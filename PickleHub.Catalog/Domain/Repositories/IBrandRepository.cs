using PickleHub.Catalog.Domain.Entities;

namespace PickleHub.Catalog.Domain.Repositories
{
    public interface IBrandRepository
    {
        Task<Brand?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Brand>> GetAllAsync(CancellationToken ct = default);
        Task<bool> HasProductsAsync(Guid id, CancellationToken ct = default);

        void Add(Brand brand);
        void Update(Brand brand);
        void Remove(Brand brand);
    }
}
