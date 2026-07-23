using MediatR;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Products.GetProductVariant
{
    public record GetProductVariantQuery(
     Guid ProductId,
     Guid VariantId) : IRequest<ProductVariantDto>;

    public class GetProductVariantHandler
        : IRequestHandler<GetProductVariantQuery, ProductVariantDto>
    {
        private readonly IProductRepository _productRepository;

        public GetProductVariantHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductVariantDto> Handle(
            GetProductVariantQuery request, CancellationToken ct)
        {
            var product = await _productRepository
                .GetByIdWithDetailAsync(request.ProductId, ct)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm.");

            var variant = product.Variants
                .FirstOrDefault(v => v.Id == request.VariantId)
                ?? throw new NotFoundException("Không tìm thấy biến thể sản phẩm.");

            // Lấy ảnh đại diện của variant (nếu có), fallback sang ảnh đầu tiên của product
            var image = product.Images
                .FirstOrDefault(i => i.VariantId == variant.Id)
                ?? product.Images.OrderBy(i => i.SortOrder).FirstOrDefault();

            return new ProductVariantDto
            {
                Id = variant.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                Sku = variant.Sku,
                AttributesJson = variant.AttributesJson,
                Price = variant.Price,
                ImageUrl = image?.Url
            };
        }
    }
}
