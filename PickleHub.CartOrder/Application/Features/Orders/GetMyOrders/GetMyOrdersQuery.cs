using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Application.Features.Orders.DTOs;
using PickleHub.CartOrder.Infrastructure.Persistence;

namespace PickleHub.CartOrder.Application.Features.Orders.GetMyOrders;

// Query lấy danh sách đơn hàng (tóm tắt) của User đang đăng nhập.
public record GetMyOrdersQuery(Guid UserId) : IRequest<List<OrderSummaryDto>>;

public class GetMyOrdersQueryHandler(CartOrderDbContext db) 
    : IRequestHandler<GetMyOrdersQuery, List<OrderSummaryDto>>
{
    public async Task<List<OrderSummaryDto>> Handle(GetMyOrdersQuery request, CancellationToken ct)
    {
        var orders = await db.Orders
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummaryDto
            {
                OrderId = o.Id,
                TotalPrice = o.TotalPrice,
                Status = o.Status.ToString(),       
                CreatedAt = o.CreatedAt,
                ItemCount = o.Items.Count   
            })
            .ToListAsync(ct);

        return orders;
    }
}