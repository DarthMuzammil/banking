using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface IInvestmentRepository
{
    Task<IReadOnlyList<InvestmentHolding>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
