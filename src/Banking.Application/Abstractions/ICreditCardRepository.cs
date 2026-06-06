using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface ICreditCardRepository
{
    Task<IReadOnlyList<CreditCard>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CreditCard?> GetByIdAsync(Guid cardId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditCardSpending>> GetSpendingByCardIdAsync(Guid cardId, CancellationToken cancellationToken = default);
}
