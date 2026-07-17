using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleHub.CartOrder.Application.Features.Cart.AddItem;
using PickleHub.CartOrder.Application.Features.Cart.DTOs;
using PickleHub.CartOrder.Application.Features.Cart.GetCart;
using PickleHub.CartOrder.Application.Features.Cart.RemoveItem;
using PickleHub.CartOrder.Application.Features.Cart.UpdateItem;

namespace PickleHub.CartOrder.Controllers;

[ApiController]
[Route("cart")]
[Authorize] // Yêu cầu đăng nhập thông qua JWT Bearer Token
public class CartController(ISender mediator) : ControllerBase
{
    // GET /cart -> Lấy chi tiết giỏ hàng hiện tại
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetCartQuery(userId), ct);
        return Ok(result);
    }

    // POST /cart/items -> Thêm sản phẩm vào giỏ hàng
    [HttpPost("items")]
    public async Task<ActionResult<Guid>> AddItem(
        [FromBody] AddCartItemRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new AddCartItemCommand(userId, request.ProductId, request.Quantity), ct);
        return Ok(result);
    }

    // PUT /cart/items -> Cập nhật số lượng sản phẩm trong giỏ
    [HttpPut("items")]
    public async Task<ActionResult<Guid>> UpdateItem(
        [FromBody] UpdateCartItemRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new UpdateCartItemCommand(userId, request.ProductId, request.Quantity), ct);
        return Ok(result);
    }

    // DELETE /cart/items/{productId} -> Xóa sản phẩm khỏi giỏ hàng
    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<Guid>> RemoveItem(
        Guid productId, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new RemoveCartItemCommand(userId, productId), ct);
        return Ok(result);
    }

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
public record AddCartItemRequest(Guid ProductId, int Quantity);
public record UpdateCartItemRequest(Guid ProductId, int Quantity);
