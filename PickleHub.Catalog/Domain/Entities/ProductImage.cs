using PickleHub.Common.Domain;

namespace PickleHub.Catalog.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public Product? Product { get; set; }
    }
}
