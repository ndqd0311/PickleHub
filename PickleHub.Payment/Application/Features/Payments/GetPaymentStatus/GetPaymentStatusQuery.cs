using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Payment.Domain.Enums;
using PickleHub.Payment.Infrastructure.Persistence;

namespace PickleHub.Payment.Application.Features.Payments.GetPaymentStatus;

public record GetPaymentStatusQuery(Guid OrderId) : IRequest<PaymentStatusDto>;

public record PaymentStatusDto(Guid OrderId, string Status, decimal Amount, DateTime? PaidAt = null);

public class GetPaymentStatusQueryHandler(PaymentDbContext db) : IRequestHandler<GetPaymentStatusQuery, PaymentStatusDto>
{
    public async Task<PaymentStatusDto> Handle(GetPaymentStatusQuery request, CancellationToken ct)
    {
        var payment = await db.Payments
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId, ct);

        if (payment is null)
        {
            return new PaymentStatusDto(request.OrderId, "None", 0);
        }

        return new PaymentStatusDto(
            payment.OrderId,
            payment.Status.ToString(),
            payment.Amount,
            payment.PaidAt
        );
    }
}
