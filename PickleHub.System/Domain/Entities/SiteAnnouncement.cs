using PickleHub.Common.Domain;

namespace PickleHub.System.Domain.Entities
{
    public class SiteAnnouncement : BaseEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Content { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;

        public DateTime? StartsAt { get; private set; }
        public DateTime? EndsAt { get; private set; }
        public bool IsVisible =>
            IsActive && (StartsAt == null || StartsAt <= DateTime.UtcNow)
                     && (EndsAt == null || EndsAt >= DateTime.UtcNow);

        private SiteAnnouncement() { }
        public static SiteAnnouncement Create(
            string title,
            string content,
            bool isActive = true,
            DateTime? startsAt = null,
            DateTime? endsAt = null
            )
        {
            return new SiteAnnouncement
            {
                Title = title.Trim(),
                Content = content.Trim(),
                IsActive = isActive,
                StartsAt = startsAt,
                EndsAt = endsAt
            };
        }

        public void Update(
            string title,
            string content,
            bool isActive,
            DateTime? startsAt,
            DateTime? endsAt
            )
        {
            Title = title.Trim();
            Content = content.Trim();
            IsActive = isActive;
            StartsAt = startsAt;
            EndsAt = endsAt;
            SetUpdated();
        }

        public void Activate()
        {
            IsActive = true;
            SetUpdated();
        }

        public void Deactivate()
        {
            IsActive = false;
            SetUpdated();
        }
    }
}
