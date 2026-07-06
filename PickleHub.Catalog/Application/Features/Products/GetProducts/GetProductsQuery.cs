using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Application.Features.Brands.DTOs;
using PickleHub.Catalog.Application.Features.Categories.DTOs;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Domain.Enums;
using PickleHub.Catalog.Infrastructure.Persistence;
using PickleHub.Common.DTOs;

namespace PickleHub.Catalog.Application.Features.ProductFeature.GetProducts
{
    public record GetProductsQuery(
     string? Keyword,
     Guid? CategoryId,
     Guid? BrandId,
     decimal? MinPrice,
     decimal? MaxPrice,
     int Page = 1,
     int PageSize = 20
 ) : IRequest<PagedResult<ProductListDto>>;

    public class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductListDto>>
    {
        private readonly CatalogDbContext _db;

        public GetProductsHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<ProductListDto>> Handle(GetProductsQuery request, CancellationToken ct)
        {
            var query = _db.Products.AsNoTracking().Where(p => p.Status == ProductStatus.Active);

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                query = query.Where(p => p.Name.Contains(request.Keyword));
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);
            }

            if (request.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == request.BrandId.Value);
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice <= request.MaxPrice.Value);
            }

            var totalItems = await query.CountAsync(ct);

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    BasePrice = p.BasePrice,
                    ThumbnailUrl = p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                    Brand = p.Brand == null ? null : new BrandDto { Id = p.Brand.Id, Name = p.Brand.Name },
                    Category = p.Category == null ? null : new CategorySummaryDto { Id = p.Category.Id, Name = p.Category.Name }
                })
                .ToListAsync(ct);

            return new PagedResult<ProductListDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalItems = totalItems
            };
        }
    }
}
