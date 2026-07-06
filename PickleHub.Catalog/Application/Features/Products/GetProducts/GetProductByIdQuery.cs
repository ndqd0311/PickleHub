

using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Application.Features.Brands.DTOs;
using PickleHub.Catalog.Application.Features.Categories.DTOs;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.ProductFeature.GetProducts
{
    public record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDetailDto>;

    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto?>
    {
        private readonly CatalogDbContext _db;

        public GetProductByIdHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task<ProductDetailDto?> Handle(GetProductByIdQuery request, CancellationToken ct)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, ct);

            if (product == null)
            {
                throw new NotFoundException("Sản phẩm không tồn tại.");
            }

            return new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                BasePrice = product.BasePrice,
                Status = product.Status.ToString(),
                SpecsJson = product.SpecsJson,
                Category = product.Category == null ? null
                   : new CategorySummaryDto { Id = product.Category.Id, Name = product.Category.Name },
                Brand = product.Brand == null ? null
                   : new BrandDto { Id = product.Brand.Id, Name = product.Brand.Name },
                Images = product.Images.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    Url = i.Url,
                    VariantId = i.VariantId,
                    SortOrder = i.SortOrder
                }).ToList(),
                Variants = product.Variants.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Sku = v.Sku,
                    AttributesJson = v.AttributesJson,
                    Price = v.Price
                }).ToList()
            };
        }
    }
}
