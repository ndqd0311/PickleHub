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

namespace PickleHub.CartOrder.Application.Features.Orders.UpdateOrderStatusCommand;

// Command cập nhật trạng thái order.
public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus OrderStatus) : IRequest<OrderStatus>;

public class UpdateOrderStatusCommandHandler(
    CartOrderDbContext db,
    ICustomerClient customerClient, 
    IPublishEndpoint publishEndpoint
) : IRequestHandler<UpdateOrderStatusCommand, OrderStatus>
{
    public async Task<OrderStatus> Handle(UpdateOrderStatusCommand request, CancellationToken ct)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);
        
        if (order is null)
        {
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        }

        // Truy vấn Email/Tên khách hàng từ Customer Service để phục vụ thông báo
        var customer = await customerClient.GetCustomerDetailsAsync(order.UserId, ct);
        var customerEmail = customer?.Email ?? string.Empty;
        var customerName = customer?.FullName ?? order.ShippingFullName;

        var oldStatus = order.Status;
        order.Status = request.OrderStatus;
        await db.SaveChangesAsync(ct);

        // Publish sự kiện thay đổi trạng thái đơn hàng phục vụ cho Notification Service và Audit Log
        await publishEndpoint.Publish(new OrderStatusUpdatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.UserId,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            OldStatus = oldStatus.ToString(),
            NewStatus = order.Status.ToString(),
            UpdatedAt = DateTime.UtcNow
        }, ct);
        
        return order.Status;
    }
}