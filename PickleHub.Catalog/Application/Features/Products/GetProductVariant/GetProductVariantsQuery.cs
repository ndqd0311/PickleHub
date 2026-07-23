using MediatR;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Products.GetProductVariant
{
    public record GetProductVariantsQuery(Guid ProductId)
     : IRequest<List<ProductVariantDto>>;

    public class GetProductVariantsHandler
        : IRequestHandler<GetProductVariantsQuery, List<ProductVariantDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetProductVariantsHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<ProductVariantDto>> Handle(
            GetProductVariantsQuery request, CancellationToken ct)
        {
            var product = await _productRepository
                .GetByIdWithDetailAsync(request.ProductId, ct)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm.");

            return product.Variants.Select(variant =>
            {
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
            }).ToList();
        }
    }
}
