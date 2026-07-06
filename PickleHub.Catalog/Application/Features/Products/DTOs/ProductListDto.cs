using PickleHub.Catalog.Application.Features.Brands.DTOs;
using PickleHub.Catalog.Application.Features.Categories.DTOs;

namespace PickleHub.Catalog.Application.Features.Products.DTOs
{
    public class ProductListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string? ThumbnailUrl { get; set; }
        public BrandDto? Brand { get; set; }
        public CategorySummaryDto? Category { get; set; }
    }

}
