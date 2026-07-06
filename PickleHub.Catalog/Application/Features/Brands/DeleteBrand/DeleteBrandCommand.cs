using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Brands.DeleteBrand
{
    public record DeleteBrandCommand(Guid Id) : IRequest;

    public class DeleteBrandHandler : IRequestHandler<DeleteBrandCommand>
    {
        private readonly CatalogDbContext _db;
        public DeleteBrandHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeleteBrandCommand request, CancellationToken ct)
        {
            var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == request.Id, ct);
            if (brand == null)
            {
                throw new KeyNotFoundException("Thương hiệu không tồn tại.");
            }
            var hasProducts = await _db.Products.AnyAsync(p => p.BrandId == request.Id, ct);

            if (hasProducts)
            {
                throw new ConflictException("Không thể xóa thương hiệu vì có sản phẩm liên quan.");
            }

            _db.Brands.Remove(brand);
            await _db.SaveChangesAsync(ct);
        }
    }
}
