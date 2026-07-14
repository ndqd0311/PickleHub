using MediatR;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Products.PublishProduct
{
    public record PublishProductCommand(Guid Id) : IRequest;

    public class PublishProductHandler : IRequestHandler<PublishProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PublishProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(PublishProductCommand request, CancellationToken ct)
        {
            var product = await _productRepository.GetByIdWithDetailAsync(request.Id, ct)
                ?? throw new NotFoundException("Sản phẩm không tồn tại.");

            product.Publish();

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
