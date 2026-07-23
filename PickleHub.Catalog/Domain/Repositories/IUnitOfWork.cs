namespace PickleHub.Catalog.Domain.Repositories
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
