using PickleHub.Common.Domain;

namespace PickleHub.Catalog.Domain.Entities
{
    public class Brand : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;

        private Brand() { }

        public static Brand Create(string name)
        {
            return new Brand { Name = name.Trim() };
        }

        public void Update(string name)
        {
            Name = name;
            SetUpdated();
        }
    }
}
