using PickleHub.Common.Domain;

namespace PickleHub.Catalog.Domain.Entities
{
    public class Category : BaseEntity
    {      
        public string Name { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }

        public ICollection<Category> Children { get; set; } = new List<Category>();
    }
}
