using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PickleHub.Payment.Domain.Entities;
using PickleHub.Payment.Application.Features.DTOs;
using PickleHub.Payment.Domain.Enums;
using PickleHub.Payment.Infrastructure.Persistence;

namespace PickleHub.Payment.Application.Features.Payments.CreatePayment;

// Command yêu cầu tạo link thanh toán mới.
public record CreatePaymentCommand(
    Guid OrderId,
    decimal Amount
) : IRequest<CreatePaymentResponse>;

//tạo link thanh toán 
public class CreatePaymentCommandHandler(
    PaymentDbContext db,
    PayOSClient payOsClient,
    IConfiguration config
) : IRequestHandler<CreatePaymentCommand, CreatePaymentResponse>
{
    public async Task<CreatePaymentResponse> Handle(CreatePaymentCommand request, CancellationToken ct)
    {
        // 1. Sinh mã đơn hàng ngẫu nhiên và kiểm tra tránh trùng lặp trong DB
        long orderCode;
        bool isDuplicate;
        do
        {
            orderCode = Random.Shared.Next(1000000, 99999999);
            isDuplicate = await db.Payments.AnyAsync(p => p.OrderCode == orderCode, ct);
        } while (isDuplicate);

        // 2. Lấy URL điều hướng sau khi thanh toán thành công/hủy từ file cấu hình appsettings.Development.json
        var returnUrl = config["PayOS:ReturnUrl"] ?? "http://localhost:3000/payment/success";
        var cancelUrl = config["PayOS:CancelUrl"] ?? "http://localhost:3000/payment/cancel";

        // 3. Khởi tạo dữ liệu thanh toán gửi lên PayOS
        var paymentData = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = (long)request.Amount,
            Description = $"Thanh toán đơn hàng #{orderCode}",
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl,
            Items = new List<PaymentLinkItem>()
        };

        try
        {
            // 4. Gọi API của SDK PayOS v2 để tạo link thanh toán
            var createPaymentResult = await payOsClient.PaymentRequests.CreateAsync(paymentData);

            // 5. Lưu lịch sử giao dịch vào Database nội bộ ở trạng thái Processing (Chờ thanh toán)
            var paymentRecord = new PickleHub.Payment.Domain.Entities.Payments
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                OrderCode = orderCode,
                PaymentLinkId = createPaymentResult.PaymentLinkId,
                Amount = request.Amount,
                Method = "PayOS",
                Status = PaymentStatus.Processing,
                CreatedAt = DateTime.UtcNow
            };

            db.Payments.Add(paymentRecord);
            await db.SaveChangesAsync(ct);

            // 6. Trả về kết quả đầu ra
            return new CreatePaymentResponse(
                paymentRecord.Id,
                createPaymentResult.CheckoutUrl,
                createPaymentResult.PaymentLinkId
            );
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi kết nối với cổng thanh toán PayOS: {ex.Message}", ex);
        }
    }
}
