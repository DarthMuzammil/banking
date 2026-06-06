using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface ICustomerSettingsRepository
{
    Task<CustomerSettings?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task UpsertAsync(CustomerSettings settings, CancellationToken cancellationToken = default);
}
