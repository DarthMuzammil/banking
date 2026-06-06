using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface IRefreshTokenService
{
    Task<string> IssueAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Customer?> ValidateAndRevokeAsync(string refreshToken, CancellationToken cancellationToken = default);
}
