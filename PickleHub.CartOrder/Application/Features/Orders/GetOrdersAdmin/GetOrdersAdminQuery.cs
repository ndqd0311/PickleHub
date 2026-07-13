using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Application.Features.Orders.DTOs;
using PickleHub.CartOrder.Domain.Enums;
using PickleHub.CartOrder.Infrastructure.Persistence;
using PickleHub.Common.DTOs;

namespace PickleHub.CartOrder.Application.Features.Orders.GetOrdersAdmin;

// Query lấy danh sách toàn bộ đơn hàng trong hệ thống (dành cho Admin).
// Hỗ trợ lọc theo trạng thái, tìm kiếm từ khóa, và phân trang.
public record GetOrdersAdminQuery(
    OrderStatus? Status,
    string? SearchKeyword,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<OrderSummaryDto>>;

public class GetOrdersAdminQueryHandler(CartOrderDbContext db) 
    : IRequestHandler<GetOrdersAdminQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<PagedResult<OrderSummaryDto>> Handle(GetOrdersAdminQuery request, CancellationToken ct)
    {
        var query = db.Orders.AsQueryable();

        //Lọc theo trạng thái đơn hàng (nếu có)
        if (request.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Status.Value);
        }

        //Tìm kiếm theo Tên khách hàng nhận hoặc Số điện thoại (nếu có)
        if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
        {
            var keyword = request.SearchKeyword.Trim().ToLower();
            query = query.Where(o => o.ShippingFullName.ToLower().Contains(keyword) || 
                                     o.ShippingPhone.Contains(keyword));
        }

        //Đếm tổng số bản ghi thỏa mãn điều kiện lọc
        var totalItems = await query.CountAsync(ct);

        //Phân trang và lấy dữ liệu rút gọn (Projection)
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSummaryDto
            {
                OrderId = o.Id,
                TotalPrice = o.TotalPrice,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt,
                ItemCount = o.Items.Count
            })
            .ToListAsync(ct);

        //Trả về kết quả phân trang chuẩn
        return new PagedResult<OrderSummaryDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems
        };
    }
}
