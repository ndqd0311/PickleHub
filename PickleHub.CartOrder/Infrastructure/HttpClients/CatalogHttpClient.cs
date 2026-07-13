using System.Net;
using System.Net.Http.Json;
using PickleHub.CartOrder.Domain.Interfaces;

namespace PickleHub.CartOrder.Infrastructure.HttpClients;

// Thực hiện cuộc gọi HTTP vật lý đến Catalog Service
public class CatalogHttpClient(HttpClient httpClient) : ICatalogClient
{
    public async Task<bool> ProductExistsAsync(Guid productId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"products/{productId}", ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            throw new Exception("Không thể kết nối đến Catalog Service để kiểm tra sản phẩm.");
        }
    }

    public async Task<CatalogProductDto?> GetProductDetailsAsync(Guid productId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"products/{productId}", ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<CatalogProductDto>(cancellationToken: ct);
        }
        catch (HttpRequestException)
        {
            throw new Exception("Không thể kết nối đến Catalog Service để lấy thông tin sản phẩm.");
        }
    }
}
