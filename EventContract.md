# PickleHub — Event Contract
**Phiên bản:** 1.0  
**Ngày:** 01/07/2026  
**Áp dụng cho:** Tất cả service sử dụng RabbitMQ + MassTransit

---

## Tổng quan

Hệ thống dùng **RabbitMQ + MassTransit** cho giao tiếp bất đồng bộ giữa các service. Mỗi event được publish lên một exchange, các service subscribe queue tương ứng.

**Nguyên tắc:**
- Publisher không biết ai subscribe — chỉ publish và quên
- Consumer xử lý idempotent — nhận cùng event 2 lần không được gây lỗi
- Event payload không thay đổi sau khi đã chốt — nếu cần thêm field, thêm field mới (không xóa field cũ)

---

## Cấu hình RabbitMQ

```json
// appsettings.json — dùng chung cho mọi service
"RabbitMQ": {
  "Host": "localhost",
  "VirtualHost": "/",
  "Username": "guest",
  "Password": "guest"
}
```

---

## Quy ước đặt tên

| Thành phần | Format | Ví dụ |
|-----------|--------|-------|
| Exchange | `picklehub.{domain}` | `picklehub.orders` |
| Queue | `picklehub.{domain}.{consumer}` | `picklehub.orders.inventory` |
| Event class | `{Entity}{Action}Event` | `OrderCreatedEvent` |
| Namespace | `PickleHub.Common.Events.{Domain}` | `PickleHub.Common.Events.Orders` |

**Tất cả event record đặt trong project `PickleHub.Common`** — cả 2 người đều reference chung, tránh định nghĩa lệch nhau.

---

## Events từ Cart & Order Service

> **Publisher:** Người 2 (Cart & Order Service)  
> **Subscribers:** Người 1 (Inventory, Audit Log) + Người 2 (Notification)

---

### OrderCreatedEvent

**Khi nào publish:** Đơn hàng được tạo thành công (trạng thái `ChoXacNhan`).

```csharp
namespace PickleHub.Common.Events.Orders;

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
```

**Consumers:**

| Service | Queue | Xử lý |
|---------|-------|-------|
| Inventory | `picklehub.orders.inventory` | Trừ tồn kho theo từng `ProductVariantId` + `Quantity` |
| Notification | `picklehub.orders.notification` | Gửi email xác nhận + web notification cho customer |
| Audit Log | `picklehub.orders.auditlog` | Ghi log "Đơn hàng được tạo" |

---

### OrderStatusUpdatedEvent

**Khi nào publish:** Admin cập nhật trạng thái đơn hàng (`ChoXacNhan` → `DaXacNhan` → `DangGiao` → `HoanThanh`).

```csharp
namespace PickleHub.Common.Events.Orders;

public record OrderStatusUpdatedEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;

    // Chỉ có giá trị khi NewStatus = "DangGiao"
    public string? ShippingProvider { get; init; }   // "GHTK" | "GHN" | "ViettelPost"
    public string? TrackingNumber { get; init; }
    public string? TrackingUrl { get; init; }

    public DateTime UpdatedAt { get; init; }
}
```

**Consumers:**

| Service | Queue | Xử lý |
|---------|-------|-------|
| Notification | `picklehub.orders.notification` | Gửi email + web notification trạng thái mới |
| Audit Log | `picklehub.orders.auditlog` | Ghi log thay đổi trạng thái |

---

### OrderCancelledEvent

**Khi nào publish:** Customer tự hủy hoặc Admin hủy đơn hàng.

```csharp
namespace PickleHub.Common.Events.Orders;

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
```

**Consumers:**

| Service | Queue | Xử lý |
|---------|-------|-------|
| Inventory | `picklehub.orders.inventory` | Hoàn kho theo từng `ProductVariantId` + `Quantity` |
| Notification | `picklehub.orders.notification` | Gửi email thông báo hủy + web notification |
| Audit Log | `picklehub.orders.auditlog` | Ghi log "Đơn hàng bị hủy" |

---

## Events từ Customer Service

> **Publisher:** Người 1 (Customer Service)  
> **Subscribers:** Người 2 (Notification)

---

### CustomerBlockedEvent

**Khi nào publish:** Admin block tài khoản customer.

```csharp
namespace PickleHub.Common.Events.Customers;

public record CustomerBlockedEvent
{
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public bool IsBlocked { get; init; }
    public DateTime OccurredAt { get; init; }
}
```

**Consumers:**

| Service | Queue | Xử lý |
|---------|-------|-------|
| Notification | `picklehub.customers.notification` | Gửi email thông báo tài khoản bị khóa |
| Audit Log | `picklehub.customers.auditlog` | Ghi log thao tác block/unblock |

---

## Events từ Payment Service

> **Publisher:** Người 2 (Payment Service)  
> **Subscribers:** Người 2 (Cart & Order Service)

---

### PaymentCompletedEvent

**Khi nào publish:** PayOS webhook callback thành công, thanh toán được xác nhận.

```csharp
namespace PickleHub.Common.Events.Payments;

public record PaymentCompletedEvent
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Method { get; init; } = string.Empty; // "PayOS" | "COD"
    public DateTime PaidAt { get; init; }
}
```

**Consumers:**

| Service | Queue | Xử lý |
|---------|-------|-------|
| Cart & Order | `picklehub.payments.order` | Cập nhật `PaymentStatus = Paid` trong đơn hàng |
| Audit Log | `picklehub.payments.auditlog` | Ghi log thanh toán thành công |

---

### PaymentFailedEvent

**Khi nào publish:** Thanh toán PayOS thất bại hoặc timeout.

```csharp
namespace PickleHub.Common.Events.Payments;

public record PaymentFailedEvent
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; }
}
```

**Consumers:**

| Service | Queue | Xử lý |
|---------|-------|-------|
| Cart & Order | `picklehub.payments.order` | Cập nhật `PaymentStatus = Failed` |
| Notification | `picklehub.payments.notification` | Gửi email thông báo thanh toán thất bại |

---

## Cách implement MassTransit

### Publisher (ví dụ Order Service publish OrderCreatedEvent)

```csharp
// Inject IPublishEndpoint từ MassTransit
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IPublishEndpoint _publishEndpoint;
    // ... các dependency khác

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // ... tạo order

        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerEmail = customerEmail,
            // ... fill các field
        }, ct);

        return orderDto;
    }
}
```

### Consumer (ví dụ Inventory subscribe OrderCreatedEvent)

```csharp
// Implement IConsumer<T> từ MassTransit
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        foreach (var item in message.Items)
        {
            await _inventoryRepository
                .DeductStockAsync(item.ProductVariantId, item.Quantity);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
```

### Đăng ký MassTransit trong Program.cs

```csharp
builder.Services.AddMassTransit(x =>
{
    // Đăng ký tất cả consumer trong assembly
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.ConfigureEndpoints(ctx);
    });
});
```

---

## Checklist trước khi release

- [ ] Event record đã được thêm vào `PickleHub.Common` (không định nghĩa riêng trong từng service)
- [ ] Publisher test đã publish đúng payload
- [ ] Consumer test idempotent (nhận 2 lần không bị lỗi/duplicate)
- [ ] Queue name đúng convention `picklehub.{domain}.{consumer}`
- [ ] Không có breaking change trên event đã publish (chỉ thêm field mới, không xóa)
