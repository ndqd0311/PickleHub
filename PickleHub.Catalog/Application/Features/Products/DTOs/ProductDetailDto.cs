using PickleHub.Catalog.Application.Features.Brands.DTOs;
using PickleHub.Catalog.Application.Features.Categories.DTOs;

namespace PickleHub.Catalog.Application.Features.Products.DTOs
{
    public class ProductDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SpecsJson { get; set; } = "{}";
        public CategorySummaryDto? Category { get; set; }
        public BrandDto? Brand { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
    }
  
}
