using MediatR;
using PickleHub.Catalog.Application.Features.Brands.DTOs;
using PickleHub.Catalog.Domain.Entities;
using PickleHub.Catalog.Domain.Repositories;

namespace PickleHub.Catalog.Application.Features.Brands.CreateBrand
{
    public record CreateBrandCommand(string Name) : IRequest<BrandDto>;

    public class CreateBrandHandler : IRequestHandler<CreateBrandCommand, BrandDto>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;
        public CreateBrandHandler(IBrandRepository brandRepository, IUnitOfWork unitOfWork)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BrandDto> Handle(CreateBrandCommand request, CancellationToken ct)
        {
            var brand = Brand.Create(request.Name);
            _brandRepository.Add(brand);
            await _unitOfWork.SaveChangesAsync(ct);
            return new BrandDto { Id = brand.Id, Name = brand.Name };
        }
    }
}
