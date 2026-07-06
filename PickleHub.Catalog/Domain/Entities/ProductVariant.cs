using PickleHub.Common.Domain;

namespace PickleHub.Catalog.Domain.Entities
{
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string AttributesJson { get; set; } = "{}";
        public decimal Price { get; set; }
        public Product? Product { get; set; }
    }
}
