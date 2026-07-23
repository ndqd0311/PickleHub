using MediatR;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.Exceptions;
using PickleHub.Common.Interfaces;

namespace PickleHub.Catalog.Application.Features.Products.RemoveImage
{
    public record RemoveProductImageCommand(Guid ProductId, Guid ImageId) : IRequest;

    public class RemoveProductImageHandler : IRequestHandler<RemoveProductImageCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storage;

        public RemoveProductImageHandler(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            IStorageService storage)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _storage = storage;
        }

        public async Task Handle(RemoveProductImageCommand request, CancellationToken ct)
        {
            var product = await _productRepository.GetByIdWithDetailAsync(request.ProductId, ct)
                ?? throw new NotFoundException("Sản phẩm không tồn tại.");

            var image = product.Images.FirstOrDefault(i => i.Id == request.ImageId)
                ?? throw new NotFoundException("Ảnh không tồn tại trong sản phẩm này.");

            var publicId = image.PublicId;

            product.RemoveImage(request.ImageId);

            await _unitOfWork.SaveChangesAsync(ct);

            if (!string.IsNullOrEmpty(publicId))
                await _storage.DeleteAsync(publicId);
        }
    }
}
