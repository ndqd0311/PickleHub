namespace PickleHub.CartOrder.Domain.Interfaces;

public interface ICatalogClient
{
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken ct = default);
    Task<CatalogProductDto?> GetProductDetailsAsync(Guid productId, CancellationToken ct = default);
}

public record CatalogProductDto(
    Guid Id,
    string Name,
    decimal Price,
    string ImageUrl
);
