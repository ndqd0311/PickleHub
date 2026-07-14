using PickleHub.Common.Domain;

namespace PickleHub.Catalog.Domain.Entities
{
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public string Sku { get; private set; } = string.Empty;
        public string AttributesJson { get; private set; } = "{}";
        public decimal Price { get; private set; }
        public Product? Product { get; private set; }

        private ProductVariant() { }
        public static ProductVariant Create(Guid productId, string sku,
            string attributesJson, decimal price)
        {
            return new ProductVariant
            {
                ProductId = productId,
                Sku = sku,
                AttributesJson = string.IsNullOrWhiteSpace(attributesJson) ? "{}" : attributesJson,
                Price = price
            };
        }

        public void Update(string sku, string attributesJson, decimal price)
        {
            Sku = sku;
            AttributesJson = string.IsNullOrWhiteSpace(attributesJson) ? "{}" : attributesJson;
            Price = price;
            SetUpdated();
        }
    }
}
