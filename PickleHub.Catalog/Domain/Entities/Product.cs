using PickleHub.Catalog.Domain.Enums;
using PickleHub.Common.Domain;

namespace PickleHub.Catalog.Domain.Entities
{
    public class Product : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public Guid BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Draft;
        public string SpecsJson { get; set; } = "{}";

        public Category? Category { get; set; }
        public Brand? Brand { get; set; }
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    }


}
