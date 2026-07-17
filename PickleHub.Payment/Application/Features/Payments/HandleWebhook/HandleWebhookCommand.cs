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
    // 2. Tạo Constructor để Inject các dependencies này vào handler.
    // 3. Trong hàm Handle:
    //    - B1: Xác thực Signature của webhook bằng:
    //          var verifiedData = await payOSClient.Webhooks.VerifyAsync(request.WebhookBody);
    //    - B2: Tìm giao dịch trong DB khớp với verifiedData.OrderCode.
    //    - B3: Nếu không tìm thấy, trả về false hoặc ném KeyNotFoundException.
    //    - B4: Nếu trạng thái giao dịch đã xử lý rồi (!= Processing) -> Trả về true (Idempotent).
    //    - B5: Cập nhật trạng thái giao dịch:
    //          + Nếu verifiedData.Code == "00" (Thành công): Cập nhật Succeeded, publish PaymentCompletedEvent.
    //          + Nếu ngược lại: Cập nhật Failed, publish PaymentFailedEvent.
    //    - B6: Lưu thay đổi vào DB: await db.SaveChangesAsync(ct);
    //    - B7: Trả về true.

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

            if (payment.Status != PaymentStatus.Processing)
            {
                return true;
            }

            if (verifiedData.Code == "00")
            {
                payment.Status = PaymentStatus.Succeeded;

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
