using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Domain.Enums;
using PickleHub.Catalog.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Products.UpdateProduct
{
    public record UpdateProductCommand(
        Guid Id,
        string Name,
        string Description,
        Guid CategoryId,
        Guid BrandId,
        decimal BasePrice,
        string SpecsJson,
        string Status
    ) : IRequest<ProductDetailDto>;

    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductDetailDto>
    {
        private readonly CatalogDbContext _db;

        public UpdateProductHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task<ProductDetailDto> Handle(UpdateProductCommand request, CancellationToken ct)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (product == null)
            {
                throw new NotFoundException("Sản phẩm không tồn tại.");
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.CategoryId = request.CategoryId;
            product.BrandId = request.BrandId;
            product.BasePrice = request.BasePrice;
            product.SpecsJson = string.IsNullOrWhiteSpace(request.SpecsJson) ? "{}" : request.SpecsJson;
            product.Status = Enum.TryParse<ProductStatus>(request.Status, true, out var status) ? status : product.Status;
            product.UpdatedAt = DateTime.UtcNow;

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
