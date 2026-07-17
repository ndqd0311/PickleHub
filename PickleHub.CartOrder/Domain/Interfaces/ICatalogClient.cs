using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PickleHub.CartOrder.Domain.Interfaces;

public interface ICatalogClient
{
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken ct = default);
    Task<CatalogProductDto?> GetProductDetailsAsync(Guid productId, CancellationToken ct = default);
}

/// <summary>
/// DTO khớp chính xác với ProductDetailDto của Catalog Service.
/// </summary>
public class CatalogProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public List<CatalogProductImageDto> Images { get; set; } = new();
    public List<CatalogProductVariantDto> Variants { get; set; } = new();
}

public class CatalogProductImageDto
{
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsSizeChart { get; set; }
}

public class CatalogProductVariantDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
