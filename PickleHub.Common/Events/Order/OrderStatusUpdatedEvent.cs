using System;

namespace PickleHub.Common.Events.Orders;

/// <summary>
/// Event phát ra khi Admin cập nhật trạng thái đơn hàng.
/// Khớp chính xác với mô tả trong EventContract.md.
/// </summary>
public record OrderStatusUpdatedEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;

    // Chỉ có giá trị khi NewStatus = "DangGiao" (Shipping)
    public string? ShippingProvider { get; init; }   // "GHTK" | "GHN" | "ViettelPost"
    public string? TrackingNumber { get; init; }
    public string? TrackingUrl { get; init; }

    public DateTime UpdatedAt { get; init; }
}
