using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface IAccountRepository
{
    Task<IReadOnlyList<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Account account, CancellationToken cancellationToken = default);
}