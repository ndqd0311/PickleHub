using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PickleHub.Payment.Domain.Entities;
using PickleHub.Payment.Application.Features.DTOs;
using PickleHub.Payment.Domain.Enums;
using PickleHub.Payment.Infrastructure.Persistence;

using PickleHub.Payment.Domain.Interfaces;

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
    IConfiguration config,
    IOrderClient orderClient
) : IRequestHandler<CreatePaymentCommand, CreatePaymentResponse>
{
    public async Task<CreatePaymentResponse> Handle(CreatePaymentCommand request, CancellationToken ct)
    {
        // 0. Xác thực số tiền và đơn hàng từ CartOrder Service (Bảo mật giao dịch)
        var order = await orderClient.GetOrderDetailsAsync(request.OrderId, ct);
        if (order is null)
        {
            throw new KeyNotFoundException("Không tìm thấy thông tin đơn hàng bên dịch vụ CartOrder.");
        }

        if (order.TotalAmount != request.Amount)
        {
            throw new InvalidOperationException($"Số tiền yêu cầu ({request.Amount} VNĐ) không khớp với giá trị thực tế của đơn hàng ({order.TotalAmount} VNĐ).");
        }

        // 1. Kiểm tra Idempotency: Nếu đơn hàng đã từng yêu cầu thanh toán
        var existingPayment = await db.Payments.FirstOrDefaultAsync(p => p.OrderId == request.OrderId, ct);
        if (existingPayment is not null)
        {
            if (existingPayment.Status == PaymentStatus.Paid)
            {
                throw new InvalidOperationException("Đơn hàng này đã được thanh toán thành công trước đó.");
            }

            try
            {
                // Lấy lại chi tiết link thanh toán hiện tại từ PayOS để kiểm tra trạng thái
                var linkInfo = await payOsClient.PaymentRequests.GetAsync(existingPayment.OrderCode);
                
                if (linkInfo.Status == PayOS.Models.V2.PaymentRequests.PaymentLinkStatus.Paid)
                {
                    existingPayment.Status = PaymentStatus.Paid;
                    await db.SaveChangesAsync(ct);
                    throw new InvalidOperationException("Đơn hàng này đã được thanh toán thành công trước đó.");
                }

                if (linkInfo.Status == PayOS.Models.V2.PaymentRequests.PaymentLinkStatus.Cancelled ||
                    linkInfo.Status == PayOS.Models.V2.PaymentRequests.PaymentLinkStatus.Expired)
                {
                    throw new Exception("Link thanh toán cũ đã bị hủy hoặc hết hạn.");
                }

                return new CreatePaymentResponse(
                    existingPayment.Id,
                    $"https://pay.payos.vn/web/{existingPayment.PaymentLinkId}",
                    existingPayment.PaymentLinkId
                );
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception)
            {
                // Nếu link hết hạn hoặc lỗi trên cổng PayOS, chúng ta sẽ cho phép tạo link mới bên dưới bằng cách xóa bản ghi cũ
                db.Payments.Remove(existingPayment);
                await db.SaveChangesAsync(ct);
            }
        }

        // 2. Sinh mã đơn hàng ngẫu nhiên và kiểm tra tránh trùng lặp trong DB
        long orderCode;
        bool isDuplicate;
        do
        {
            orderCode = Random.Shared.Next(1000000, 99999999);
            isDuplicate = await db.Payments.AnyAsync(p => p.OrderCode == orderCode, ct);
        } while (isDuplicate);

        // 3. Lấy URL điều hướng sau khi thanh toán thành công/hủy từ file cấu hình
        var returnUrl = config["PayOS:ReturnUrl"] ?? "http://localhost:3000/payment/success";
        var cancelUrl = config["PayOS:CancelUrl"] ?? "http://localhost:3000/payment/cancel";

        // 4. Khởi tạo dữ liệu thanh toán gửi lên PayOS
        var paymentData = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = (long)request.Amount,
            Description = $"Thanh toán đơn hàng #{orderCode}",
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl,
            Items = new List<PaymentLinkItem>()
        };

        // 5. Gọi API của SDK PayOS v2 để tạo link thanh toán
        PayOS.Models.V2.PaymentRequests.CreatePaymentLinkResponse? createPaymentResult = null;
        try
        {
            createPaymentResult = await payOsClient.PaymentRequests.CreateAsync(paymentData);
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi kết nối với cổng thanh toán PayOS để tạo link: {ex.Message}", ex);
        }

        try
        {
            // 6. Lưu lịch sử giao dịch vào Database nội bộ ở trạng thái Processing (Chờ thanh toán)
            var paymentRecord = new PickleHub.Payment.Domain.Entities.Payments
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                OrderCode = orderCode,
                PaymentLinkId = createPaymentResult.PaymentLinkId,
                Amount = request.Amount,
                Method = "PayOS",
                Status = PaymentStatus.Unpaid,
                CreatedAt = DateTime.UtcNow
            };

            db.Payments.Add(paymentRecord);
            await db.SaveChangesAsync(ct);

            // 7. Trả về kết quả đầu ra
            return new CreatePaymentResponse(
                paymentRecord.Id,
                createPaymentResult.CheckoutUrl,
                createPaymentResult.PaymentLinkId
            );
        }
        catch (Exception ex)
        {
            // Compensating Action: Gọi huỷ link thanh toán trên PayOS nếu không thể lưu lịch sử vào DB cục bộ
            try
            {
                await payOsClient.PaymentRequests.CancelAsync(orderCode, "Lỗi hệ thống lưu trữ giao dịch cục bộ.");
            }
            catch
            {
                // Bỏ qua lỗi huỷ để tránh nuốt exception chính
            }

            throw new Exception($"Lỗi khi lưu trữ lịch sử giao dịch thanh toán: {ex.Message}", ex);
        }
    }
}
