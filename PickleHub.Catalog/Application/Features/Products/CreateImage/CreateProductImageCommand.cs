using MediatR;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Domain.Entities;
using PickleHub.Catalog.Infrastructure.Persistence;

namespace PickleHub.Catalog.Application.Features.Products.CreateImage
{
    public record CreateProductImageCommand(
        Guid ProductId,
        Guid? VariantId,
        string Url,
        int sortOrder
    ) : IRequest<ProductImageDto>;

    public class CreateProductImageHandler : IRequestHandler<CreateProductImageCommand, ProductImageDto>
    {
        private readonly CatalogDbContext _db;

        public CreateProductImageHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task<ProductImageDto> Handle(CreateProductImageCommand request, CancellationToken ct)
        {
            var image = new ProductImage
            {
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                Url = request.Url,
                SortOrder = request.sortOrder
            };

            _db.ProductImages.Add(image);
            await _db.SaveChangesAsync(ct);

            return new ProductImageDto
            {
                Id = image.Id,
                Url = image.Url,
                VariantId = image.VariantId,
                SortOrder = image.SortOrder
            };
        }
    }
}
