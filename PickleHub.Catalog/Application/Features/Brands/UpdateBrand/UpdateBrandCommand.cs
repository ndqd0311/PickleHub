using MediatR;
using PickleHub.Catalog.Application.Features.Brands.DTOs;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Brands.UpdateBrand
{
    public record UpdateBrandCommand(Guid Id, string Name) : IRequest<BrandDto>;
    public class UpdateBrandHandler : IRequestHandler<UpdateBrandCommand, BrandDto>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UpdateBrandHandler(IBrandRepository brandRepository, IUnitOfWork unitOfWork)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<BrandDto> Handle(UpdateBrandCommand request, CancellationToken ct)
        {
            var brand = await _brandRepository.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException($"Không tìm thấy thương hiệu với Id: {request.Id}");

            brand.Update(request.Name);
            _brandRepository.Update(brand);
            await _unitOfWork.SaveChangesAsync(ct);

            return new BrandDto
            {
                Id = request.Id,
                Name = request.Name
            };
        }
    }
}
