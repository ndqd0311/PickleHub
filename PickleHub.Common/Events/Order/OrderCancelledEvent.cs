using System;
using System.Collections.Generic;
using PickleHub.Common.Events.Order;

namespace PickleHub.Common.Events.Orders;

// Event phát ra khi một đơn hàng bị hủy bởi Khách hàng hoặc Admin.
public record OrderCancelledEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public List<OrderItemPayload> Items { get; init; } = new(); // để Inventory hoàn kho
    public string CancelledBy { get; init; } = string.Empty;   // "Customer" | "Admin"
    public string? CancelReason { get; init; }
    public DateTime CancelledAt { get; init; }
}
