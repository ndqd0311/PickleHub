using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Domain.Entities;
using PickleHub.CartOrder.Domain.Interfaces;
using PickleHub.CartOrder.Infrastructure.Persistence;

namespace PickleHub.CartOrder.Application.Features.Cart.AddItem;

public record AddCartItemCommand(Guid UserId, Guid ProductId, int Quantity) : IRequest<Guid>;

public class AddCartItemHandler(CartOrderDbContext db, ICatalogClient catalogClient ) : IRequestHandler<AddCartItemCommand, Guid>
{
    public async Task<Guid> Handle(AddCartItemCommand request, CancellationToken ct)
    {
        var productExists = await catalogClient.ProductExistsAsync(request.ProductId, ct);
        if (!productExists)
        {
            throw new KeyNotFoundException($"Sản phẩm với ID {request.ProductId} không tồn tại hoặc đã bị xóa.");
        }
        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);

        if (cart is null)
        {
            cart = new Domain.Entities.Cart
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Items = new List<CartItem>()
            };
            db.Carts.Add(cart);
        }
        
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            existingItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };
            cart.Items.Add(existingItem);
        }
        await db.SaveChangesAsync(ct);
        return existingItem.Id;
    }
}
