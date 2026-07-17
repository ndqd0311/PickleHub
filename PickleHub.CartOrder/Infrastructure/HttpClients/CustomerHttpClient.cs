using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using PickleHub.CartOrder.Domain.Interfaces;

namespace PickleHub.CartOrder.Infrastructure.HttpClients;

// Thực hiện cuộc gọi HTTP vật lý đến Customer Service sử dụng header bảo mật nội bộ X-Internal-Service.
public class CustomerHttpClient(HttpClient httpClient) : ICustomerClient
{
    public async Task<CustomerDto?> GetCustomerDetailsAsync(Guid customerId, CancellationToken ct = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"customers/{customerId}");
            
            // Bypass auth check bằng cách đính kèm header X-Internal-Service
            request.Headers.Add("X-Internal-Service", "true");

            var response = await httpClient.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<CustomerDto>(cancellationToken: ct);
        }
        catch (HttpRequestException)
        {
            throw new Exception("Không thể kết nối đến Customer Service để lấy thông tin khách hàng.");
        }
    }
}
