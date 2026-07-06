using MediatR;
using PickleHub.Catalog.Application.Features.Brands.DTOs;
using PickleHub.Catalog.Domain.Entities;
using PickleHub.Catalog.Infrastructure.Persistence;

namespace PickleHub.Catalog.Application.Features.Brands.CreateBrand
{
    public record CreateBrandCommand(string Name) : IRequest<BrandDto>;

    public class CreateBrandHandler : IRequestHandler<CreateBrandCommand, BrandDto>
    {
        private readonly CatalogDbContext _db;

        public CreateBrandHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task<BrandDto> Handle(CreateBrandCommand request, CancellationToken ct)
        {
            var brand = new Brand { Name = request.Name };
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync(ct);

            return new BrandDto { Id = brand.Id, Name = brand.Name };
        }
    }
}
