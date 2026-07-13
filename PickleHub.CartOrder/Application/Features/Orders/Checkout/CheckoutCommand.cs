using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Domain.Entities;
using PickleHub.CartOrder.Domain.Enums;
using PickleHub.CartOrder.Domain.Interfaces;
using PickleHub.CartOrder.Infrastructure.Persistence;
using PickleHub.Common.Events;

namespace PickleHub.CartOrder.Application.Features.Orders.Checkout;

// Command đặt hàng (Checkout).
public record CheckoutCommand(
    Guid UserId,
    string ShippingFullName,
    string ShippingPhone,
    string ShippingAddress,
    string ShippingCity
) : IRequest<Guid>;

public class CheckoutCommandHandler(
    CartOrderDbContext db,
    ICatalogClient catalogClient,
    IInventoryClient inventoryClient,
    IPublishEndpoint publishEndpoint
) : IRequestHandler<CheckoutCommand, Guid>
{
    public async Task<Guid> Handle(CheckoutCommand request, CancellationToken ct)
    {
        //Lấy giỏ hàng của người dùng kèm danh sách sản phẩm
        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);

        if (cart is null || cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Giỏ hàng của bạn đang trống, không thể thực hiện đặt hàng.");
        }

        //Chuẩn bị thực thể Order mới
        var orderId = Guid.NewGuid();
        var orderItems = new List<OrderItem>();
        var totalAmount = 0m;

        //Khởi tạo danh sách event item gửi đi
        var eventItems = new List<OrderCreatedItem>();

        //Lặp qua từng item trong giỏ để kiểm tra nghiệp vụ chéo qua các Service khác
        foreach (var cartItem in cart.Items)
        {
            //Kiểm tra tồn tại & Lấy giá thật từ Catalog Service (Sync HTTP Call)
            var product = await catalogClient.GetProductDetailsAsync(cartItem.ProductId, ct);
            if (product is null)
            {
                throw new KeyNotFoundException($"Sản phẩm với ID {cartItem.ProductId} không khả dụng.");
            }

            //Kiểm tra tồn kho từ Inventory Service (Sync HTTP Call)
            var isAvailable = await inventoryClient.CheckStockAsync(cartItem.ProductId, cartItem.Quantity, ct);
            if (!isAvailable)
            {
                throw new InvalidOperationException($"Sản phẩm '{product.Name}' không đủ số lượng tồn kho.");
            }

            //Tính toán tiền và tạo OrderItem (Snapshot Pattern)
            var unitPrice = product.Price;
            totalAmount += unitPrice * cartItem.Quantity;

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = cartItem.ProductId,
                ProductName = product.Name, 
                UnitPrice = unitPrice,      
                Quantity = cartItem.Quantity
            });

            eventItems.Add(new OrderCreatedItem(
                cartItem.ProductId,
                product.Name,
                unitPrice,
                cartItem.Quantity
            ));
        }

        //Lưu đơn hàng mới vào Database
        var order = new Order
        {
            Id = orderId,
            UserId = request.UserId,
            TotalPrice = totalAmount,
            Status = OrderStatus.Pending, 
            ShippingFullName = request.ShippingFullName,
            ShippingPhone = request.ShippingPhone,
            ShippingAddress = request.ShippingAddress,
            ShippingCity = request.ShippingCity,
            CreatedAt = DateTime.UtcNow,
            Items = orderItems
        };

        db.Orders.Add(order);
        db.CartItems.RemoveRange(cart.Items);
        await db.SaveChangesAsync(ct);

        //Publish OrderCreatedEvent (Async Integration Event)
        // Lệnh này gửi thông điệp vào RabbitMQ để Inventory Service trừ kho, Notification Service gửi mail xác nhận.
        await publishEndpoint.Publish(new OrderCreatedEvent(
            orderId,
            request.UserId,
            totalAmount,
            eventItems,
            request.ShippingFullName,
            request.ShippingPhone,
            request.ShippingAddress,
            request.ShippingCity,
            order.CreatedAt
        ), ct);

        return orderId;
    }
}
