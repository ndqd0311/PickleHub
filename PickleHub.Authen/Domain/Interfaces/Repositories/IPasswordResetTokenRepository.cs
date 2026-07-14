using PickleHub.Authen.Domain.Entities;

namespace PickleHub.Authen.Domain.Interfaces.Repositories
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken ct = default);
        Task<List<PasswordResetToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);

        void Add(PasswordResetToken token);
    }
}
