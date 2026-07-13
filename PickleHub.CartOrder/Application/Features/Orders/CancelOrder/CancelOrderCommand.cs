using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Domain.Enums;
using PickleHub.CartOrder.Infrastructure.Persistence;
using PickleHub.Common.Events;

namespace PickleHub.CartOrder.Application.Features.Orders.CancelOrder;

/// Command hủy đơn hàng.
public record CancelOrderCommand(Guid OrderId, Guid UserId, string Reason ) : IRequest<bool>;

public class CancelOrderCommandHandler(CartOrderDbContext db, IPublishEndpoint publishEndpoint ) : IRequestHandler<CancelOrderCommand, bool>
{
    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        //Tìm đơn hàng cần hủy và kiểm tra quyền sở hữu (scoped by UserId)
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, ct);

        if (order is null)
        {
            throw new KeyNotFoundException("Không tìm thấy đơn hàng yêu cầu hoặc bạn không có quyền hủy đơn này.");
        }

        //Kiểm tra trạng thái đơn hàng (Business Rule)
        if (order.Status == OrderStatus.Shipping || order.Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException("Đơn hàng đang được giao hoặc đã hoàn thành, không thể hủy.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Đơn hàng này đã được hủy trước đó.");
        }

        //Cập nhật trạng thái sang Cancelled
        order.Status = OrderStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        //Publish Event "OrderCancelled" qua Message Broker
        await publishEndpoint.Publish(new OrderCancelledEvent(
            order.Id,
            request.Reason,
            DateTime.UtcNow
        ), ct);

        return true;
    }
}
