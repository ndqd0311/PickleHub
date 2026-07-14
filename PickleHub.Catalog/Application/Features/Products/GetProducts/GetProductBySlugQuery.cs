using MediatR;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Application.Mappings;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Products.GetProducts
{
    public record GetProductBySlugQuery(string Slug) : IRequest<ProductDetailDto>;

    public class GetProductBySlugHandler : IRequestHandler<GetProductBySlugQuery, ProductDetailDto>
    {
        private readonly IProductRepository _productRepository;
        public GetProductBySlugHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDetailDto> Handle(GetProductBySlugQuery request, CancellationToken ct)
        {
            var product = await _productRepository.GetBySlugAsync(request.Slug, ct)
                ?? throw new NotFoundException("Sản phẩm không tồn tại.");

            return product.MapToDetailDto();
        }
    }
}
