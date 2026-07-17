using System;
using System.Collections.Generic;

namespace PickleHub.Common.Events.Orders;

/// <summary>
/// Event phát ra khi một đơn hàng được đặt thành công (Checkout).
/// Khớp chính xác với mô tả trong EventContract.md.
/// </summary>
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;

    // Địa chỉ giao hàng (đã snapshot)
    public string ShippingFullName { get; init; } = string.Empty;
    public string ShippingPhone { get; init; } = string.Empty;
    public string ShippingAddress { get; init; } = string.Empty; // full address string

    public List<OrderItemPayload> Items { get; init; } = new();
    public decimal TotalAmount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty; // "COD" | "PayOS"
    public DateTime CreatedAt { get; init; }
}

public record OrderItemPayload
{
    public Guid ProductVariantId { get; init; }
    public string ProductNameSnapshot { get; init; } = string.Empty;
    public string VariantAttributesSnapshot { get; init; } = string.Empty; // "Màu: Xanh, Size: 42"
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
