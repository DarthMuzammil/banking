using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string?> GetPasswordHashAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Customer customer, string passwordHash, CancellationToken cancellationToken = default);
}
