using MediatR;
using PickleHub.Catalog.Application.Features.Brands.DTOs;
using PickleHub.Catalog.Application.Features.Categories.DTOs;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Application.Mappings;
using PickleHub.Catalog.Domain.Entities;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Products.GetProducts
{
    public record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDetailDto>;

    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto>
    {
        private readonly IProductRepository _productRepository;
        public GetProductByIdHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<ProductDetailDto> Handle(GetProductByIdQuery request, CancellationToken ct)
        {
            var product = await _productRepository.GetByIdWithDetailAsync(request.ProductId, ct)
                ?? throw new NotFoundException("Sản phẩm không tồn tại.");

            return product.MapToDetailDto();
        }
    }
}
