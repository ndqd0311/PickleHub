using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Infrastructure.Persistence;

namespace PickleHub.CartOrder.Application.Features.Cart.UpdateItem;

// Command cập nhật số lượng sản phẩm trong giỏ hàng.
public record UpdateCartItemCommand(Guid UserId, Guid ProductId, int Quantity) : IRequest<Guid>;

public class UpdateCartItemCommandHandler(CartOrderDbContext db) 
    : IRequestHandler<UpdateCartItemCommand, Guid>
{
    public async Task<Guid> Handle(UpdateCartItemCommand request, CancellationToken ct)
    {
        var item = await db.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(
                i => i.Cart.UserId == request.UserId && 
                     i.ProductId == request.ProductId,
                ct);

        if (item is null)
        {
            throw new KeyNotFoundException("Không tìm thấy sản phẩm này trong giỏ hàng.");
        }
        
        item.Quantity = request.Quantity;
        await db.SaveChangesAsync(ct);
        return item.ProductId;
    }
}