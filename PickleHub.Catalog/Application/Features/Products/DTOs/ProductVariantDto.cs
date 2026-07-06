namespace PickleHub.Catalog.Application.Features.Products.DTOs
{
    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string AttributesJson { get; set; } = "{}";
        public decimal Price { get; set; }
    }
}
