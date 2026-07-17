using System;
using System.Threading;
using System.Threading.Tasks;

namespace PickleHub.CartOrder.Domain.Interfaces;

// Contract để giao tiếp đồng bộ với Customer Service.
public interface ICustomerClient
{
    Task<CustomerDto?> GetCustomerDetailsAsync(Guid customerId, CancellationToken ct = default);
}

public record CustomerDto(
    Guid Id,
    string Email,
    string FullName,
    string? PhoneNumber
);
