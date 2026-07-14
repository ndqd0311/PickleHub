using MediatR;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.Exceptions;
using PickleHub.Common.Interfaces;

namespace PickleHub.Catalog.Application.Features.Products.AddProductImage
{
    public record AddProductImageCommand(
       Guid ProductId,
       IFormFile File,
       int SortOrder,
       Guid? VariantId = null,
       bool IsSizeChart = false
    ) : IRequest<ProductImageDto>;

    public class AddProductImageHandler : IRequestHandler<AddProductImageCommand, ProductImageDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storage;

        public AddProductImageHandler(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            IStorageService storage)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _storage = storage;
        }

        public async Task<ProductImageDto> Handle(AddProductImageCommand request, CancellationToken ct)
        {
            var product = await _productRepository.GetByIdWithDetailAsync(request.ProductId, ct)
                ?? throw new NotFoundException("Sản phẩm không tồn tại.");

            await using var stream = request.File.OpenReadStream();

            var uploadResult = await _storage.UploadAsync(
                fileStream: stream,
                fileName: request.File.FileName,
                folder: "products",
                resourceType: "image",
                ct: ct);

            var image = product.AddImage(
                uploadResult.PublicId,
                uploadResult.SecureUrl,
                request.SortOrder,
                request.VariantId,
                request.IsSizeChart);



            try
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }
            catch
            {
                await _storage.DeleteAsync(uploadResult.PublicId);
                throw;
            }

            return new ProductImageDto
            {
                Id = image.Id,
                PublicId = image.PublicId,
                Url = image.Url,
                VariantId = image.VariantId,
                SortOrder = image.SortOrder,
                IsSizeChart = image.IsSizeChart
            };
        }
    }
}
