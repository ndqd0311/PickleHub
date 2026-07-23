using System;
using System.Threading;
using System.Threading.Tasks;

namespace PickleHub.Payment.Domain.Interfaces;

public interface IOrderClient
{
    Task<OrderDetailsDto?> GetOrderDetailsAsync(Guid orderId, CancellationToken ct = default);
}

public class OrderDetailsDto
{
    public Guid Id { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}
