using MediatR;
using PickleHub.Common.Exceptions;
using PickleHub.System.Domain.Repositories;

namespace PickleHub.System.Application.Features.Announcements.DeleteAnnouncement
{
    public record DeleteAnnouncementCommand(Guid AnnouncementId) : IRequest;
    public class DeleteAnnouncementHandler : IRequestHandler<DeleteAnnouncementCommand>
    {
        private readonly ISiteAnnouncementRepository _announcementRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAnnouncementHandler(
            ISiteAnnouncementRepository announcementRepository,
            IUnitOfWork unitOfWork)
        {
            _announcementRepository = announcementRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteAnnouncementCommand request, CancellationToken ct)
        {
            var announcement = await _announcementRepository.GetByIdAsync(request.AnnouncementId, ct)
                ?? throw new NotFoundException("Không tìm thấy thông báo.");

            _announcementRepository.Remove(announcement);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
