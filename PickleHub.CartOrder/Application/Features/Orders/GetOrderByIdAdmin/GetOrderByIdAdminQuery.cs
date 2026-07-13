using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Application.Features.Orders.DTOs;
using PickleHub.CartOrder.Infrastructure.Persistence;

namespace PickleHub.CartOrder.Application.Features.Orders.GetOrderByIdAdmin;

/// Query xem chi tiết đơn hàng bất kỳ của Admin (không cần validate UserId).
public record GetOrderByIdAdminQuery(Guid OrderId) : IRequest<OrderDto>;

public class GetOrderByIdAdminQueryHandler(CartOrderDbContext db) 
    : IRequestHandler<GetOrderByIdAdminQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdAdminQuery request, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

        if (order is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy đơn hàng với ID {request.OrderId}.");
        }

        return new OrderDto
        {
            OrderId = order.Id,
            TotalPrice = order.TotalPrice,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            
            ShippingFullName = order.ShippingFullName,
            ShippingPhone = order.ShippingPhone,
            ShippingAddress = order.ShippingAddress,
            ShippingCity = order.ShippingCity,

            Items = order.Items.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };
    }
}
