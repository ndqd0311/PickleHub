using MediatR;
using PickleHub.Catalog.Domain.Enums;
using PickleHub.Catalog.Infrastructure.Persistence;
using PickleHub.Catalog.Domain;
using PickleHub.Catalog.Domain.Entities;
using PickleHub.Catalog.Application.Features.Products.DTOs;

namespace PickleHub.Catalog.Application.Features.Products.CreateProduct
{
    public record CreateProductCommand(
        string Name,
        string Description,
        Guid CategoryId,
        Guid BrandId,
        decimal BasePrice,
        string SpecsJson
    ) : IRequest<ProductDetailDto>;

    public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDetailDto>
    {
        private readonly CatalogDbContext _db;

        public CreateProductHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task<ProductDetailDto> Handle(CreateProductCommand request, CancellationToken ct)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                BasePrice = request.BasePrice,
                SpecsJson = string.IsNullOrWhiteSpace(request.SpecsJson) ? "{}" : request.SpecsJson,
                Status = ProductStatus.Draft
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync(ct);

            return new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                BasePrice = product.BasePrice,
                Status = product.Status.ToString(),
                SpecsJson = product.SpecsJson
            };
        }
    }
}
