namespace PickleHub.Common.Events;

// Event được phát ra khi một đơn hàng bị hủy bỏ.
// Inventory Service sẽ lắng nghe event này để cộng lại số lượng hàng vào kho.
public record OrderCancelledEvent(
    Guid OrderId,
    string Reason,
    DateTime CancelledAt
);
