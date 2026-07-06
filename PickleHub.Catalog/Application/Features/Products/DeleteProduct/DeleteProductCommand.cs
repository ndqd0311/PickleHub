using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Domain.Enums;
using PickleHub.Catalog.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Products.DeleteProduct
{
    public record DeleteProductCommand(Guid Id) : IRequest;

    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand>
    {
        private readonly CatalogDbContext _db;

        public DeleteProductHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeleteProductCommand request, CancellationToken ct)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (product == null)
            {
                throw new NotFoundException("Sản phẩm không tồn tại.");
            }

            product.Status = ProductStatus.Hidden;
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
