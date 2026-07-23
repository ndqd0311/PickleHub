using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Domain.Entities;
using PickleHub.CartOrder.Domain.Enums;
using PickleHub.CartOrder.Domain.Interfaces;
using PickleHub.CartOrder.Infrastructure.Persistence;
using PickleHub.Common.Events.Order;
using PickleHub.Common.Events.Orders;

namespace PickleHub.CartOrder.Application.Features.Orders.Checkout;

// Command đặt hàng (Checkout).
public record CheckoutCommand(
    Guid UserId,
    Guid AddressId,
    string PaymentMethod = "COD",
    string? Note = null
) : IRequest<CheckoutResponse>;

public record CheckoutResponse(
    Guid OrderId,
    decimal TotalAmount,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    string? PaymentUrl = null
);

public class CheckoutCommandHandler(
    CartOrderDbContext db,
    ICatalogClient catalogClient,
    IInventoryClient inventoryClient,
    ICustomerClient customerClient,
    ISystemClient systemClient,
    IPaymentClient paymentClient,
    IPublishEndpoint publishEndpoint
) : IRequestHandler<CheckoutCommand, CheckoutResponse>
{
    public async Task<CheckoutResponse> Handle(CheckoutCommand request, CancellationToken ct)
    {
        // Lấy giỏ hàng của người dùng kèm danh sách sản phẩm
        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);

        if (cart is null || cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Giỏ hàng của bạn đang trống, không thể thực hiện đặt hàng.");
        }

        // Gọi Customer Service lấy địa chỉ từ Sổ địa chỉ (AddressBook q) để Snapshot
        var address = await customerClient.GetAddressByIdAsync(request.AddressId, ct);
        if (address is null)
        {
            throw new KeyNotFoundException($"Địa chỉ với ID {request.AddressId} không tồn tại trong Sổ địa chỉ.");
        }

        // Gọi Customer Service lấy thông tin Email khách hàng
        var customer = await customerClient.GetCustomerDetailsAsync(request.UserId, ct);
        var customerEmail = customer?.Email ?? string.Empty;
        var customerName = customer?.FullName ?? address.FullName;

        // Khởi tạo danh sách OrderItem và kiểm tra tồn kho & giá
        var orderId = Guid.NewGuid();
        var orderItems = new List<OrderItem>();
        var subtotal = 0m;
        var eventItems = new List<OrderItemPayload>();
        var isAllStockAvailable = true;
        var reservedItems = new List<(Guid ProductVariantId, int Quantity)>();

        try
        {
            // Lặp qua từng item trong giỏ để kiểm tra nghiệp vụ chéo qua các Service khác
            foreach (var cartItem in cart.Items)
            {
                var targetVariantId = cartItem.ProductVariantId != Guid.Empty ? cartItem.ProductVariantId : cartItem.ProductId;
                
                var product = await catalogClient.GetProductDetailsAsync(targetVariantId, ct);
                if (product is null)
                {
                    throw new KeyNotFoundException($"Sản phẩm với ID {targetVariantId} không khả dụng.");
                }

                // Nếu từ đầu phát hiện không đủ tồn kho, ta sẽ không giữ chỗ nữa mà đánh dấu là thiếu hàng
                if (isAllStockAvailable)
                {
                    var reserveSuccess = await inventoryClient.ReserveStockAsync(targetVariantId, cartItem.Quantity, ct);
                    if (!reserveSuccess)
                    {
                        isAllStockAvailable = false;
                        // Giải phóng toàn bộ tồn kho đã giữ chỗ trước đó do không đủ hàng đồng bộ
                        foreach (var reserved in reservedItems)
                        {
                            await inventoryClient.ReleaseStockAsync(reserved.ProductVariantId, reserved.Quantity, ct);
                        }
                        reservedItems.Clear();
                    }
                    else
                    {
                        reservedItems.Add((targetVariantId, cartItem.Quantity));
                    }
                }

                var variant = product.Variants.FirstOrDefault(v => v.Id == targetVariantId);
                var unitPrice = variant?.Price ?? product.BasePrice;
                var itemSubtotal = unitPrice * cartItem.Quantity;
                subtotal += itemSubtotal;

                var imageUrl = product.Images
                    .Where(img => !img.IsSizeChart)
                    .OrderBy(img => img.SortOrder)
                    .FirstOrDefault()?.Url ?? cartItem.ImageUrlSnapshot;

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductVariantId = targetVariantId,
                    ProductId = product.Id,
                    ProductNameSnapshot = product.Name,
                    VariantAttributesSnapshot = variant?.Sku ?? string.Empty,
                    ImageUrlSnapshot = imageUrl,
                    UnitPrice = unitPrice,
                    Quantity = cartItem.Quantity,
                    Subtotal = itemSubtotal,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                eventItems.Add(new OrderItemPayload
                {
                    ProductVariantId = targetVariantId,
                    ProductNameSnapshot = product.Name,
                    VariantAttributesSnapshot = variant?.Sku ?? string.Empty,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice
                });
            }
        }
        catch (Exception)
        {
            // Giải phóng toàn bộ tồn kho đã giữ chỗ nếu có lỗi bất kỳ trong quá trình xử lý loop
            foreach (var reserved in reservedItems)
            {
                await inventoryClient.ReleaseStockAsync(reserved.ProductVariantId, reserved.Quantity, ct);
            }
            throw;
        }

        decimal shippingFee = await systemClient.GetDefaultShippingFeeAsync(ct);
        decimal totalAmount = subtotal + shippingFee;

        // Logic tự động xác nhận:
        // Đơn COD + Đủ hàng -> Tự động OrderStatus.Confirmed
        // Đơn PayOS HOẶC Thiếu hàng -> OrderStatus.Pending (Chờ thanh toán / Chờ Admin duyệt kho)
        var isCod = request.PaymentMethod.Equals("COD", StringComparison.OrdinalIgnoreCase);
        var initialStatus = (isCod && isAllStockAvailable) ? OrderStatus.Confirmed : OrderStatus.Pending;

        var order = new Order
        {
            Id = orderId,
            CustomerId = request.UserId,
            Status = initialStatus,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = PaymentStatus.Unpaid,
            IsStockReserved = isAllStockAvailable,
            ShippingFullName = address.FullName,
            ShippingPhone = address.PhoneNumber,
            ShippingProvince = address.Province,
            ShippingDistrict = address.District,
            ShippingWard = address.Ward,
            ShippingStreetAddress = address.StreetAddress,
            Subtotal = subtotal,
            ShippingFee = shippingFee,
            TotalAmount = totalAmount,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = orderItems
        };

        // 4. Lưu đơn hàng vào DB trước để Payment Service có thể thực hiện đối soát số tiền (verify) thành công qua HTTP
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        // 5. Nếu là đơn PayOS, gọi Payment Service để sinh liên kết thanh toán QR Code
        string? paymentUrl = null;
        if (request.PaymentMethod.Equals("PayOS", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var paymentResult = await paymentClient.CreatePaymentLinkAsync(orderId, totalAmount, ct);
                paymentUrl = paymentResult?.CheckoutUrl;
                if (string.IsNullOrEmpty(paymentUrl))
                {
                    throw new Exception("Không nhận được URL thanh toán từ cổng PayOS.");
                }
            }
            catch (Exception ex)
            {
                // Compensating Action: Xoá đơn hàng vừa lưu khỏi DB để bảo toàn trạng thái
                db.Orders.Remove(order);
                await db.SaveChangesAsync(ct);

                // Giải phóng toàn bộ tồn kho đã giữ chỗ trước đó do lỗi cổng thanh toán
                foreach (var reserved in reservedItems)
                {
                    await inventoryClient.ReleaseStockAsync(reserved.ProductVariantId, reserved.Quantity, ct);
                }

                throw new Exception($"Không thể hoàn tất Checkout do lỗi cổng thanh toán: {ex.Message}", ex);
            }
        }

        // 6. Checkout thành công -> Xoá giỏ hàng
        db.CartItems.RemoveRange(cart.Items);
        await db.SaveChangesAsync(ct);

        // Publish OrderCreatedEvent để Inventory trừ kho & Notification gửi email
        await publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = orderId,
            CustomerId = request.UserId,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            ShippingFullName = address.FullName,
            ShippingPhone = address.PhoneNumber,
            ShippingAddress = $"{address.StreetAddress}, {address.Ward}, {address.District}, {address.Province}",
            Items = eventItems,
            TotalAmount = totalAmount,
            PaymentMethod = request.PaymentMethod,
            CreatedAt = order.CreatedAt
        }, ct);

        // Nếu là đơn COD đủ kho (tự động Confirmed) -> Publish thêm OrderStatusUpdatedEvent để gửi email xác nhận đã duyệt
        if (initialStatus == OrderStatus.Confirmed)
        {
            await publishEndpoint.Publish(new OrderStatusUpdatedEvent
            {
                OrderId = orderId,
                CustomerId = request.UserId,
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                OldStatus = OrderStatus.Pending.ToString(),
                NewStatus = OrderStatus.Confirmed.ToString(),
                UpdatedAt = DateTime.UtcNow
            }, ct);
        }

        return new CheckoutResponse(
            order.Id,
            order.TotalAmount,
            order.Status.ToString(),
            order.PaymentMethod,
            order.PaymentStatus.ToString(),
            paymentUrl
        );
    }
}
