using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Application.Features.Brands.DTOs;
using PickleHub.Catalog.Infrastructure.Persistence;

namespace PickleHub.Catalog.Application.Features.Brands.GetBrand
{
    public record GetBrandsQuery() : IRequest<List<BrandDto>>;

    public class GetBrandsHandler : IRequestHandler<GetBrandsQuery, List<BrandDto>>
    {
        private readonly CatalogDbContext _db;

        public GetBrandsHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task<List<BrandDto>> Handle(GetBrandsQuery request, CancellationToken ct)
        {
            return await _db.Brands
                .AsNoTracking()
                .Select(b => new BrandDto { Id = b.Id, Name = b.Name })
                .ToListAsync(ct);
        }
    }

}
