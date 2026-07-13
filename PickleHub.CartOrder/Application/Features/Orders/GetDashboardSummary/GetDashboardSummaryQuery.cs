using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Domain.Enums;
using PickleHub.CartOrder.Infrastructure.Persistence;

namespace PickleHub.CartOrder.Application.Features.Orders.GetDashboardSummary;

// Query lấy dữ liệu tóm tắt kinh doanh hiển thị trên Admin Dashboard.
public record GetDashboardSummaryQuery : IRequest<OrderDashboardSummaryDto>;

public class GetDashboardSummaryQueryHandler(CartOrderDbContext db) 
    : IRequestHandler<GetDashboardSummaryQuery, OrderDashboardSummaryDto>
{
    public async Task<OrderDashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken ct)
    {
        //Đếm tổng số lượng đơn hàng theo từng trạng thái
        var totalOrders = await db.Orders.CountAsync(ct);
        
        var pendingCount = await db.Orders.CountAsync(o => o.Status == OrderStatus.Pending, ct);
        var completedCount = await db.Orders.CountAsync(o => o.Status == OrderStatus.Completed, ct);
        var cancelledCount = await db.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled, ct);

        //Tính tổng doanh thu (chỉ cộng tiền từ những đơn hàng đã thanh toán thành công hoặc đã hoàn thành)
        var totalRevenue = await db.Orders
            .Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Completed)
            .SumAsync(o => o.TotalPrice, ct);

        return new OrderDashboardSummaryDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            PendingOrders = pendingCount,
            CompletedOrders = completedCount,
            CancelledOrders = cancelledCount
        };
    }
}

// DTO chứa dữ liệu tóm tắt báo cáo kinh doanh cho Admin Dashboard
public class OrderDashboardSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
}
