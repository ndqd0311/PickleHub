using PickleHub.Catalog.Application.Features.Brands.DTOs;
using PickleHub.Catalog.Application.Features.Categories.DTOs;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Domain.Entities;

namespace PickleHub.Catalog.Application.Mappings
{
    public static class ProductMapping
    {
        public static ProductDetailDto MapToDetailDto(this Product product) => new()
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug.Value,
            Description = product.Description,
            BasePrice = product.BasePrice,
            Status = product.Status.ToString(),
            SpecsJson = product.SpecsJson,
            SoldCount = product.SoldCount,
            Category = product.Category is null ? null : new CategorySummaryDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                Slug = product.Category.Slug.Value
            },
            Brand = product.Brand is null ? null : new BrandDto
            {
                Id = product.Brand.Id,
                Name = product.Brand.Name
            },
            Images = product.Images
             .OrderBy(i => i.SortOrder)
             .Select(i => new ProductImageDto
             {
                 Id = i.Id,
                 PublicId = i.PublicId,
                 Url = i.Url,
                 VariantId = i.VariantId,
                 SortOrder = i.SortOrder,
                 IsSizeChart = i.IsSizeChart
             }).ToList(),
            Variants = product.Variants
             .Select(v => new ProductVariantDto
             {
                 Id = v.Id,
                 Sku = v.Sku,
                 AttributesJson = v.AttributesJson,
                 Price = v.Price
             }).ToList()
        };
    }
}
