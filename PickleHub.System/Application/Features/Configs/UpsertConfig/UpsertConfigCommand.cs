using MediatR;
using PickleHub.System.Application.Features.DTOs;
using PickleHub.System.Domain.Entities;
using PickleHub.System.Domain.Repositories;

namespace PickleHub.System.Application.Features.Configs.UpsertConfig
{
    //Dùng pattern Upsert — nếu key đã tồn tại thì update, chưa có thì create.
    public record UpsertConfigCommand (
        string Key, 
        string Value,
        string? Description) : IRequest<SystemConfigDto>;

    public class UpsertConfigCommandHandler : IRequestHandler<UpsertConfigCommand, SystemConfigDto>
    {
        private readonly ISystemConfigRepository _configRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UpsertConfigCommandHandler(ISystemConfigRepository configRepository, IUnitOfWork unitOfWork)
        {
            _configRepository = configRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<SystemConfigDto> Handle(UpsertConfigCommand request, CancellationToken ct)
        {
            var existing = await _configRepository.GetByKeyAsync(request.Key, ct);

            if (existing != null)
            {
                existing.Update(request.Value, request.Description);
            }
            else
            {
                existing = SystemConfig.Create(request.Key, request.Value, request.Description);
                _configRepository.Add(existing);
            }
            await _unitOfWork.SaveChangesAsync(ct);

            return new SystemConfigDto
            {
                Id = existing.Id,
                Key = existing.Key,
                Value = existing.Value,
                Description = existing.Description,
                UpdatedAt = existing.UpdatedAt
            };
        }
    }
}
