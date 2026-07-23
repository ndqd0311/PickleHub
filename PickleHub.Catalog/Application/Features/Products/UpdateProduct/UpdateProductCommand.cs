using MediatR;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.Exceptions;
using PickleHub.Common.ValueObjects;

namespace PickleHub.Catalog.Application.Features.Products.UpdateProduct
{
    public record UpdateProductCommand(
       Guid Id,
       string Name,
       string Description,
       Guid CategoryId,
       Guid BrandId,
       decimal BasePrice,
       string? SpecsJson
    ) : IRequest<ProductDetailDto>;

    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductDetailDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UpdateProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductDetailDto> Handle(UpdateProductCommand request, CancellationToken ct)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException("Sản phẩm không tồn tại.");

            var slug = await GenerateUniqueSlugAsync(request.Name, request.Id, ct);

            product.Update(
                request.Name,
                slug,
                request.Description,
                request.CategoryId,
                request.BrandId,
                request.BasePrice,
                request.SpecsJson ?? "{}"
            );


            await _unitOfWork.SaveChangesAsync(ct);

            return new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug.Value,
                Description = product.Description,
                BasePrice = product.BasePrice,
                Status = product.Status.ToString(),
                SpecsJson = product.SpecsJson,
            };
        }

        private async Task<Slug> GenerateUniqueSlugAsync(string name, Guid? excludeId, CancellationToken ct)
        {
            var baseSlug = Slug.Create(name);
            var candidate = baseSlug;
            var counter = 1;

            while (await _productRepository.ExistsBySlugAsync(candidate.Value, excludeId, ct))
                candidate = baseSlug.AppendSuffix(counter++);

            return candidate;
        }
    }
}
