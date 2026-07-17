using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleHub.CartOrder.Application.Features.Orders.CancelOrder;
using PickleHub.CartOrder.Application.Features.Orders.Checkout;
using PickleHub.CartOrder.Application.Features.Orders.DTOs;
using PickleHub.CartOrder.Application.Features.Orders.GetDashboardSummary;
using PickleHub.CartOrder.Application.Features.Orders.GetOrderById;
using PickleHub.CartOrder.Application.Features.Orders.GetOrderByIdAdmin;
using PickleHub.CartOrder.Application.Features.Orders.GetMyOrders;
using PickleHub.CartOrder.Application.Features.Orders.GetOrdersAdmin;
using PickleHub.CartOrder.Application.Features.Orders.UpdateOrderStatusCommand;
using PickleHub.CartOrder.Domain.Enums;
using PickleHub.Common.DTOs;

namespace PickleHub.CartOrder.Controllers;

[ApiController]
[Route("orders")]
[Authorize] // Yêu cầu đăng nhập thông qua JWT Token
public class OrderController(ISender mediator) : ControllerBase
{
    // ==========================================
    // CUSTOMER ENDPOINTS
    // ==========================================

    // POST /orders -> Đặt hàng (Checkout)
    [HttpPost]
    public async Task<ActionResult<Guid>> Checkout(
        [FromBody] CheckoutRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new CheckoutCommand(
            userId,
            request.ShippingFullName,
            request.ShippingPhone,
            request.ShippingAddress,
            request.ShippingCity
        ), ct);
        
        return Ok(result);
    }

    // GET /orders -> Xem lịch sử đơn hàng cá nhân
    [HttpGet]
    public async Task<ActionResult<List<OrderSummaryDto>>> GetMyOrders(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetMyOrdersQuery(userId), ct);
        return Ok(result);
    }

    // GET /orders/{id} -> Xem chi tiết 1 đơn hàng cá nhân (có check bảo mật UserId)
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetOrderByIdQuery(id, userId), ct);
        return Ok(result);
    }

    // PUT /orders/{id}/cancel -> Khách hàng tự hủy đơn
    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<bool>> CancelOrder(
        Guid id, [FromBody] CancelOrderRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new CancelOrderCommand(id, userId, request.Reason), ct);
        return Ok(result);
    }
    
    // ==========================================
    // ADMIN ENDPOINTS (Phân quyền Admin tự động qua JWT)
    // ==========================================

    // GET /orders/admin -> Admin lấy danh sách toàn bộ đơn hàng (phân trang + lọc)
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetOrdersAdmin(
        [FromQuery] OrderStatus? status,
        [FromQuery] string? searchKeyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetOrdersAdminQuery(status, searchKeyword, page, pageSize), ct);
        return Ok(result);
    }

    // GET /orders/admin/{id} -> Admin xem chi tiết đơn hàng bất kỳ
    [HttpGet("admin/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderDto>> GetOrderByIdAdmin(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderByIdAdminQuery(id), ct);
        return Ok(result);
    }

    // PUT /orders/admin/{id}/status -> Admin cập nhật trạng thái giao hàng/hoàn thành
    [HttpPut("admin/{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderStatus>> UpdateOrderStatus(
        Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateOrderStatusCommand(id, request.Status), ct);
        return Ok(result);
    }

    // GET /orders/admin/dashboard-summary -> Admin xem thống kê doanh số doanh thu
    [HttpGet("admin/dashboard-summary")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderDashboardSummaryDto>> GetDashboardSummary(CancellationToken ct)
    {
        var result = await mediator.Send(new GetDashboardSummaryQuery(), ct);
        return Ok(result);
    }
    
    // ==========================================
    // UTILITY METHODS
    // ==========================================

    private Guid GetUserId()
    {
        // Trích xuất UserId trực tiếp từ JWT Claim "sub" (hoặc NameIdentifier)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Không tìm thấy thông tin định danh người dùng trong Token.");
        }
        return userId;
    }
}

// Request Models
public record CheckoutRequest(
    string ShippingFullName,
    string ShippingPhone,
    string ShippingAddress,
    string ShippingCity
);

public record CancelOrderRequest(string Reason);

public record UpdateOrderStatusRequest(OrderStatus Status);
