using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Domain.Enums;
using PickleHub.CartOrder.Infrastructure.Persistence;

// Command cập nhật trạng thái order.
namespace PickleHub.CartOrder.Application.Features.Orders.UpdateOrderStatusCommand;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus OrderStatus) : IRequest<OrderStatus>;

public class UpdateOrderStatusCommandHandler(CartOrderDbContext db) 
    : IRequestHandler<UpdateOrderStatusCommand, OrderStatus>
{
    public async Task<OrderStatus> Handle(UpdateOrderStatusCommand request, CancellationToken ct)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);
        
        if (order is null)
        {
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        }

        order.Status = request.OrderStatus;
        await db.SaveChangesAsync(ct);
        
        return order.Status;
    }
}