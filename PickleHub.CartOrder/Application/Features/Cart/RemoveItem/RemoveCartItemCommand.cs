using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Infrastructure.Persistence;

namespace PickleHub.CartOrder.Application.Features.Cart.RemoveItem;

// Command xóa sản phẩm khỏi giỏ hàng.
public record RemoveCartItemCommand(Guid UserId, Guid ProductId) : IRequest<Guid>;

public class RemoveCartItemCommandHandler(CartOrderDbContext db) 
    : IRequestHandler<RemoveCartItemCommand, Guid>
{
    public async Task<Guid> Handle(RemoveCartItemCommand request, CancellationToken ct)
    {
        var item = await db.CartItems
            .Include(x => x.Cart)
            .FirstOrDefaultAsync(
                x => x.Cart.UserId == request.UserId && 
                     x.ProductId == request.ProductId,
                ct);

        if (item is null)
        {
            throw new KeyNotFoundException("Không tìm thấy sản phẩm này trong giỏ hàng.");
        }

        db.CartItems.Remove(item);
        await db.SaveChangesAsync(ct);
        return item.ProductId;
    }
}
