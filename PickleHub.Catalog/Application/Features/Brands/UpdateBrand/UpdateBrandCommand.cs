using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Application.Features.Brands.DTOs;
using PickleHub.Catalog.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Brands.UpdateBrand
{
    public record UpdateBrandCommand(Guid Id, string Name) : IRequest<BrandDto>;

    public class UpdateBrandHandler : IRequestHandler<UpdateBrandCommand, BrandDto>
    {
        private readonly CatalogDbContext _db;

        public UpdateBrandHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task<BrandDto> Handle(UpdateBrandCommand request, CancellationToken ct)
        {
            var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == request.Id, ct);

            if (brand == null)
            {
                throw new NotFoundException("Thương hiệu không tồn tại.");
            }

            brand.Name = request.Name;
            brand.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return new BrandDto { Id = brand.Id, Name = brand.Name };
        }
    }
}
