using PickleHub.Common.Domain;

namespace PickleHub.Authen.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public User? User { get; set; }
        public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
    }
}
