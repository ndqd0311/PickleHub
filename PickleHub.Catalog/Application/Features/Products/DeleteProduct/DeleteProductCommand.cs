using MediatR;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Products.DeleteProduct
{
    public record DeleteProductCommand(Guid Id) : IRequest;

    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteProductCommand request, CancellationToken ct)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException("Sản phẩm không tồn tại.");

            product.Hide();

            await _unitOfWork.SaveChangesAsync(ct);
           
        }
    }
}
