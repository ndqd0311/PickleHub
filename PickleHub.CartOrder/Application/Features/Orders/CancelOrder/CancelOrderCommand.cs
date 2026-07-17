using System.Linq;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Domain.Enums;
using PickleHub.CartOrder.Infrastructure.Persistence;
using PickleHub.Common.Events.Orders;

namespace PickleHub.CartOrder.Application.Features.Orders.CancelOrder;

/// <summary>
/// Command hủy đơn hàng.
/// </summary>
public record CancelOrderCommand(Guid OrderId, Guid UserId, string Reason) : IRequest<bool>;

public class CancelOrderCommandHandler(
    CartOrderDbContext db,
    IPublishEndpoint publishEndpoint
) : IRequestHandler<CancelOrderCommand, bool>
{
    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        // Tìm đơn hàng cần hủy và kiểm tra quyền sở hữu (Nạp kèm Items để gửi Event hoàn kho)
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, ct);

        if (order is null)
        {
            throw new KeyNotFoundException("Không tìm thấy đơn hàng yêu cầu hoặc bạn không có quyền hủy đơn này.");
        }

        // Kiểm tra trạng thái đơn hàng (Business Rule)
        if (order.Status == OrderStatus.Shipping || order.Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException("Đơn hàng đang được giao hoặc đã hoàn thành, không thể hủy.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Đơn hàng này đã được hủy trước đó.");
        }

        // Cập nhật trạng thái sang Cancelled
        order.Status = OrderStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        // Map danh sách items sang payload sự kiện phục vụ hoàn kho bên Inventory Service
        var eventItems = order.Items.Select(item => new OrderItemPayload
        {
            ProductVariantId = item.ProductId,
            ProductNameSnapshot = item.ProductName,
            VariantAttributesSnapshot = string.Empty,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        }).ToList();

        // Publish Event "OrderCancelled" qua Message Broker theo đặc tả chuẩn EventContract.md
        await publishEndpoint.Publish(new OrderCancelledEvent
        {
            OrderId = order.Id,
            CustomerId = order.UserId,
            CustomerName = order.ShippingFullName,
            CustomerEmail = string.Empty,
            Items = eventItems,
            CancelledBy = "Customer",
            CancelReason = request.Reason,
            CancelledAt = DateTime.UtcNow
        }, ct);

        return true;
    }
}
