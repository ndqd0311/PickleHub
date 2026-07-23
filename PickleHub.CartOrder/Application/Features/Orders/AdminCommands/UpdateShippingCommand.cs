using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Domain.Enums;
using PickleHub.CartOrder.Domain.Interfaces;
using PickleHub.CartOrder.Infrastructure.Persistence;
using PickleHub.Common.Events.Orders;

namespace PickleHub.CartOrder.Application.Features.Orders.AdminCommands;

public record UpdateShippingCommand(
    Guid OrderId,
    string ShippingProvider,
    string TrackingNumber
) : IRequest<bool>;

public class UpdateShippingCommandHandler(
    CartOrderDbContext db,
    ICustomerClient customerClient,
    IPublishEndpoint publishEndpoint
) : IRequestHandler<UpdateShippingCommand, bool>
{
    public async Task<bool> Handle(UpdateShippingCommand request, CancellationToken ct)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);
        if (order is null)
        {
            throw new KeyNotFoundException("Không tìm thấy đơn hàng yêu cầu.");
        }

        if (order.Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException("Đơn hàng phải ở trạng thái 'Confirmed' mới có thể cập nhật thông tin vận chuyển.");
        }

        var trackingUrl = request.ShippingProvider.ToUpper() switch
        {
            "GHTK" => $"https://i.ghtk.vn/{request.TrackingNumber}",
            "GHN" => $"https://donhang.ghn.vn/?id={request.TrackingNumber}",
            "VIETTELPOST" => $"https://viettelpost.com.vn/tra-cuu-hanh-trinh-don/{request.TrackingNumber}",
            "JANDT" or "JT" => $"https://www.jtexpress.vn/index/query/gzquery.html?bills={request.TrackingNumber}",
            _ => $"https://google.com/search?q={request.TrackingNumber}"
        };

        var oldStatus = order.Status;
        order.Status = OrderStatus.Shipping;
        order.ShippingProvider = request.ShippingProvider;
        order.TrackingNumber = request.TrackingNumber;
        order.TrackingUrl = trackingUrl;
        order.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        var customer = await customerClient.GetCustomerDetailsAsync(order.CustomerId, ct);

        await publishEndpoint.Publish(new OrderStatusUpdatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = customer?.FullName ?? order.ShippingFullName,
            CustomerEmail = customer?.Email ?? string.Empty,
            OldStatus = oldStatus.ToString(),
            NewStatus = order.Status.ToString(),
            ShippingProvider = order.ShippingProvider,
            TrackingNumber = order.TrackingNumber,
            TrackingUrl = order.TrackingUrl,
            UpdatedAt = DateTime.UtcNow
        }, ct);

        return true;
    }
}
