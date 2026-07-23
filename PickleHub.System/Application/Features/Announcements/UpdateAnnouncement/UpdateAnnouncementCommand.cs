using MediatR;
using PickleHub.Common.Exceptions;
using PickleHub.System.Application.Features.DTOs;
using PickleHub.System.Domain.Repositories;

namespace PickleHub.System.Application.Features.Announcements.UpdateAnnouncement
{
    public record UpdateAnnouncementCommand(
       Guid AnnouncementId,
       string Title,
       string Content,
       bool IsActive,
       DateTime? StartsAt,
       DateTime? EndsAt) : IRequest<SiteAnnouncementDto>;

    public class UpdateAnnouncementHandler : IRequestHandler<UpdateAnnouncementCommand, SiteAnnouncementDto>
    {
        private readonly ISiteAnnouncementRepository _announcementRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAnnouncementHandler(ISiteAnnouncementRepository announcementRepository, IUnitOfWork unitOfWork)
        {
            _announcementRepository = announcementRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<SiteAnnouncementDto> Handle(UpdateAnnouncementCommand request, CancellationToken ct)
        {
            var announcement = await _announcementRepository.GetByIdAsync(request.AnnouncementId, ct)
                ?? throw new NotFoundException("Không tìm thấy thông báo.");

            announcement.Update(
                request.Title,
                request.Content,
                request.IsActive,
                request.StartsAt,
                request.EndsAt
            );

            await _unitOfWork.SaveChangesAsync(ct);

            return new SiteAnnouncementDto
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                IsActive = announcement.IsActive,
                IsVisible = announcement.IsVisible,
                StartsAt = announcement.StartsAt,
                EndsAt = announcement.EndsAt,
                CreatedAt = announcement.CreatedAt,
                UpdatedAt = announcement.UpdatedAt
            };
        }
    }
}
