using PickleHub.Authen.Domain.Enums;
using PickleHub.Common.Domain;

namespace PickleHub.Authen.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public UserRole Role { get; private set; } = UserRole.Customer;
        public bool IsBlocked { get; private set; } = false;

        private readonly List<RefreshToken> _refreshTokens = new();
        private readonly List<PasswordResetToken> _passwordResetTokens = new();

        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
        public IReadOnlyCollection<PasswordResetToken> PasswordResetTokens => _passwordResetTokens.AsReadOnly();

        private User() { }

        public static User Create(string email, string passwordHash, UserRole role = UserRole.Customer)
        {
            return new User
            {
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                Role = role
            };
        }

        public void ChangePassword(string newPassword)
        {
            PasswordHash = newPassword;
            SetUpdated();
        }

        public void Block() 
        {
            IsBlocked = true;
            SetUpdated();
        }

        public void UnBlock() 
        {
            IsBlocked = false;
            SetUpdated();
        }
    }
}
