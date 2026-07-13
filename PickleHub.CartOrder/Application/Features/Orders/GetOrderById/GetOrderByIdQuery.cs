using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Application.Features.Orders.DTOs;
using PickleHub.CartOrder.Infrastructure.Persistence;

namespace PickleHub.CartOrder.Application.Features.Orders.GetOrderById;

// Query lấy chi tiết một đơn hàng theo ID (bảo mật theo UserId của chủ đơn).
public record GetOrderByIdQuery(Guid OrderId, Guid UserId) : IRequest<OrderDto>;

public class GetOrderByIdQueryHandler(CartOrderDbContext db) 
    : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        // Truy vấn đơn hàng kèm các item trong đơn, lọc chính xác theo OrderId và UserId chủ đơn
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, ct);

        if (order is null)
        {
            throw new KeyNotFoundException("Không tìm thấy đơn hàng yêu cầu hoặc bạn không có quyền xem đơn hàng này.");
        }

        // Map từ Entity sang DTO
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
