namespace PickleHub.Common.Events;

/// <summary>
/// Event phát ra khi một đơn hàng được đặt thành công (Checkout).
/// Inventory Service sẽ nghe để trừ kho.
/// Notification Service sẽ nghe để gửi mail xác nhận.
/// </summary>
public record OrderCreatedEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalPrice,
    List<OrderCreatedItem> Items,
    string ShippingFullName,
    string ShippingPhone,
    string ShippingAddress,
    string ShippingCity,
    DateTime CreatedAt
);

public record OrderCreatedItem(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity
);
