using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models.Webhooks;
using PickleHub.Common.Events.Payments;
using PickleHub.Payment.Domain.Enums;
using PickleHub.Payment.Infrastructure.Persistence;

namespace PickleHub.Payment.Application.Features.Payments.HandleWebhook;

// Command nhận thông báo Webhook chuyển khoản thành công/thất bại từ PayOS.
public record HandleWebhookCommand(Webhook WebhookBody) : IRequest<bool>;

// Trình xử lý nghiệp vụ đối soát và phản hồi Webhook (Handler).
public class HandleWebhookCommandHandler(
    PaymentDbContext db, 
    PayOSClient payOsClient,
    IPublishEndpoint publishEndpoint) : IRequestHandler<HandleWebhookCommand, bool>
{
    public async Task<bool> Handle(HandleWebhookCommand request, CancellationToken ct)
    {
        try
        {

            var verifiedData = await payOsClient.Webhooks.VerifyAsync(request.WebhookBody);
            var payment = await db.Payments.FirstOrDefaultAsync(p => p.OrderCode == verifiedData.OrderCode, ct);
            if (payment is null)
            {
                throw new KeyNotFoundException("Không tìm thấy giao dịch");
            }

            if (payment.Status != PaymentStatus.Unpaid)
            {
                return true;
            }

            if (verifiedData.Code == "00")
            {
                payment.Status = PaymentStatus.Paid;
                payment.PaidAt = DateTime.UtcNow;

                await publishEndpoint.Publish(new PaymentCompletedEvent
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    Method = "PayOS",
                    PaidAt = DateTime.UtcNow
                });
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.PaidAt = null;

                await publishEndpoint.Publish(new PaymentFailedEvent
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    Reason = "PayOS giao dịch thất bại hoặc bị hủy bỏ",
                    FailedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception e)
        {
            throw new Exception($"Lỗi khi kết nối với cổng thanh toán PayOS: {e.Message}", e);
        }
    }
}
