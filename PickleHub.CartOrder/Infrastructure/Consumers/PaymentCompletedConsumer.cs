using MassTransit;

namespace PickleHub.CartOrder.Infrastructure.Consumers;

// Subscribe PaymentCompletedEvent từ Payment Service
// → Cập nhật Order.Status từ Pending → Confirmed
public class PaymentCompletedConsumer : IConsumer<object>
{
    public Task Consume(ConsumeContext<object> context) => Task.CompletedTask;
}
