using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Application.Features.Cart.DTOs;
using PickleHub.CartOrder.Domain.Interfaces;
using PickleHub.CartOrder.Infrastructure.Persistence;

namespace PickleHub.CartOrder.Application.Features.Cart.GetCart;

// Query lấy thông tin chi tiết giỏ hàng của User.
public record GetCartQuery(Guid UserId) : IRequest<CartDto>;

public class GetCartHandler(CartOrderDbContext db, ICatalogClient catalogClient ) : IRequestHandler<GetCartQuery, CartDto>
{
    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken ct)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);

        if (cart is null)
        {
            return new CartDto
            {
                CartId = Guid.Empty,
                Items = new List<CartItemDto>()
            };
        }

        var cartItemDtos = new List<CartItemDto>();

        foreach (var item in cart.Items)
        {
            var productDetails = await catalogClient.GetProductDetailsAsync(item.ProductId, ct);
            if (productDetails is null) continue;
            cartItemDtos.Add(new CartItemDto
            {
                CartItemId = item.Id,
                ProductId = item.ProductId,
                ProductName = productDetails.Name,
                ImageUrl = productDetails.ImageUrl,
                UnitPrice = productDetails.Price,
                Quantity = item.Quantity
            });
        }
        
        return new CartDto
        {
            CartId = cart.Id,
            Items = cartItemDtos
        };
    }
}
