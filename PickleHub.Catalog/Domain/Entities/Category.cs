using PickleHub.Common.Domain;
using PickleHub.Common.ValueObjects;
namespace PickleHub.Catalog.Domain.Entities
{
    public class Category : BaseEntity
    {      
        public string Name { get; private set; } = string.Empty;
        public Slug Slug { get; private set; } = null!;
        public Guid? ParentId { get; private set; }

        public virtual ICollection<Category> Children { get; private set; } = new List<Category>();

        private Category() { }

        public static Category Create(string name, Slug slug, Guid? parentId = null)
        {
            return new Category
            {
                Name = name.Trim(),
                Slug = slug,
                ParentId = parentId
            };
        }

        public void Update(string name, Slug slug, Guid? parentId)
        {
            Name = name;
            Slug = slug;
            ParentId = parentId;
            SetUpdated();
        }
    }
}
