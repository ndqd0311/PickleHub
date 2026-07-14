using MediatR;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Products.RestoreProduct
{
    public record RestoreProductCommand(Guid Id) : IRequest;
    public class RestoreProductHandler : IRequestHandler<RestoreProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        public RestoreProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(RestoreProductCommand request, CancellationToken ct)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException("Sản phẩm không tồn tại.");

            product.Restore();

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
