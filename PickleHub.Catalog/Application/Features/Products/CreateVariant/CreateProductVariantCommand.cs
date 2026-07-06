using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Domain.Entities;
using PickleHub.Catalog.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Products.CreateVariant
{
    public record CreateProductVariantCommand(
        Guid ProductId,
        string Sku,
        string AttributesJson,
        decimal Price
    ): IRequest<ProductVariantDto>;

    public class CreateProductVariantHandler : IRequestHandler<CreateProductVariantCommand, ProductVariantDto>
    {
        private readonly CatalogDbContext _db;

        public CreateProductVariantHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task<ProductVariantDto> Handle(CreateProductVariantCommand request, CancellationToken ct)
        {
            var existed = await _db.ProductVariants.AnyAsync(v => v.Sku == request.Sku, ct);

            if (existed)
            {
                throw new ConflictException($"SKU '{request.Sku}' đã tồn tại");
            }

            var variant = new ProductVariant
            {
                ProductId = request.ProductId,
                Sku = request.Sku,
                AttributesJson = string.IsNullOrWhiteSpace(request.AttributesJson) ? "{}" : request.AttributesJson,
                Price = request.Price
            };

            _db.ProductVariants.Add(variant);
            await _db.SaveChangesAsync(ct);

            return new ProductVariantDto
            {
                Id = variant.Id,
                Sku = variant.Sku,
                AttributesJson = variant.AttributesJson,
                Price = variant.Price
            };
        }
    }
}
