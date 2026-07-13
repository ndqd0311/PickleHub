using System.Net;
using System.Net.Http.Json;
using PickleHub.CartOrder.Domain.Interfaces;

namespace PickleHub.CartOrder.Infrastructure.HttpClients;

// Thực hiện cuộc gọi HTTP vật lý đến Inventory Service để kiểm tra tồn kho
public class InventoryHttpClient(HttpClient httpClient) : IInventoryClient
{
    public async Task<bool> CheckStockAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/inventory/{productId}", ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            var stock = await response.Content.ReadFromJsonAsync<InventoryResponse>(cancellationToken: ct);

            return stock is not null && stock.Quantity >= quantity;
        }
        catch (HttpRequestException)
        {
            throw new Exception("Không thể kết nối đến Inventory Service để kiểm tra tồn kho.");
        }
    }
}

// DTO nội bộ đại diện cho dữ liệu trả về từ Inventory Service
public record InventoryResponse(
    Guid ProductId,
    int Quantity
);
