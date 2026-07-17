using MassTransit;
using Microsoft.EntityFrameworkCore;
using PickleHub.CartOrder.Domain.Enums;
using PickleHub.CartOrder.Infrastructure.Persistence;
using PickleHub.Common.Events.Payments;

namespace PickleHub.CartOrder.Infrastructure.Consumers;

// Lắng nghe sự kiện PaymentCompletedEvent từ RabbitMQ.
// Khi thanh toán thành công -> Chuyển trạng thái đơn hàng sang Confirmed (Đã xác nhận).
public class PaymentCompletedConsumer(CartOrderDbContext db) : IConsumer<PaymentCompletedEvent>
{
    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var message = context.Message;

        // 1. Tìm đơn hàng tương ứng trong DB của CartOrder Service
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == message.OrderId);

        if (order is null)
        {
            throw new Exception("Không thấy đơn hàng");
        }

        // 2. Kiểm tra nếu đơn hàng đã được xử lý rồi (tránh xử lý trùng lặp)
        if (order.Status != OrderStatus.Pending)
        {
            return;
        }

        // 3. Cập nhật trạng thái đơn hàng thành Confirmed (Đã thanh toán)
        order.Status = OrderStatus.Confirmed;

        // 4. Lưu thay đổi vào Database của CartOrder Service
        await db.SaveChangesAsync();
    }
}