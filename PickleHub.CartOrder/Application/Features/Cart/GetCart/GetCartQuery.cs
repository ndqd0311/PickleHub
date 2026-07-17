using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Application.Features.Cart.DTOs;
using PickleHub.CartOrder.Domain.Interfaces;
using PickleHub.CartOrder.Infrastructure.Persistence;

namespace PickleHub.CartOrder.Application.Features.Cart.GetCart;

// Query lấy thông tin chi tiết giỏ hàng của User.
public record GetCartQuery(Guid UserId) : IRequest<CartDto>;

public class GetCartHandler(CartOrderDbContext db, ICatalogClient catalogClient) : IRequestHandler<GetCartQuery, CartDto>
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
            // Gọi Catalog Service để lấy chi tiết sản phẩm
            var productDetails = await catalogClient.GetProductDetailsAsync(item.ProductId, ct);
            if (productDetails is null) continue;

            // Lấy giá của biến thể cụ thể, nếu không có thì dùng BasePrice làm giá trị mặc định
            var variant = productDetails.Variants.FirstOrDefault(v => v.Id == item.ProductId);
            var unitPrice = variant?.Price ?? productDetails.BasePrice;

            // Lấy ảnh thumbnail chính (sortOrder nhỏ nhất, không phải Size Chart)
            var imageUrl = productDetails.Images
                .Where(img => !img.IsSizeChart)
                .OrderBy(img => img.SortOrder)
                .FirstOrDefault()?.Url ?? string.Empty;

            cartItemDtos.Add(new CartItemDto
            {
                CartItemId = item.Id,
                ProductId = item.ProductId,
                ProductName = productDetails.Name,
                ImageUrl = imageUrl,
                UnitPrice = unitPrice,
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
