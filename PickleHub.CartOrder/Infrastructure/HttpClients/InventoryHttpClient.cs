using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using PickleHub.CartOrder.Domain.Interfaces;

namespace PickleHub.CartOrder.Infrastructure.HttpClients;

// Thực hiện cuộc gọi HTTP vật lý đến Inventory Service để quản lý tồn kho đồng bộ
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

    public async Task<bool> ReserveStockAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/inventory/reserve", new { ProductId = productId, Quantity = quantity }, ct);
            if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<InventoryActionResponse>(cancellationToken: ct);
            return result is not null && result.Success;
        }
        catch (HttpRequestException)
        {
            throw new Exception("Không thể kết nối đến Inventory Service để giữ chỗ tồn kho (Reserve).");
        }
    }

    public async Task<bool> ReleaseStockAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/inventory/release", new { ProductId = productId, Quantity = quantity }, ct);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<InventoryActionResponse>(cancellationToken: ct);
                return result is not null && result.Success;
            }
            return false;
        }
        catch
        {
            // Bỏ qua lỗi nhả kho để không làm ảnh hưởng đến luồng trả lỗi chính, nhưng nên log lại
            return false;
        }
    }
}

// DTO nội bộ đại diện cho dữ liệu trả về từ Inventory Service
public record InventoryResponse(
    Guid ProductId,
    int Quantity
);

public record InventoryActionResponse(
    bool Success,
    string Message
);
