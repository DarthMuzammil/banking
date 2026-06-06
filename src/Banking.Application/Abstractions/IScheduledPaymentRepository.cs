using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface IScheduledPaymentRepository
{
    Task<IReadOnlyList<ScheduledPayment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(ScheduledPayment payment, CancellationToken cancellationToken = default);
}
