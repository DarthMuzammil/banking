using System.Security.Cryptography;
using System.Text;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Banking.Domain.Enums;

namespace Banking.Infrastructure.Security;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ICustomerRepository _customerRepository;

    public RefreshTokenService(
        IRefreshTokenRepository refreshTokenRepository,
        ICustomerRepository customerRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _customerRepository = customerRepository;
    }

    public async Task<string> IssueAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = Hash(rawToken);
        var now = DateTime.UtcNow;

        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            TokenHash = tokenHash,
            ExpiresAt = now.Add(RefreshTokenLifetime),
            CreatedAt = now
        };

        await _refreshTokenRepository.CreateAsync(entity, cancellationToken);
        return rawToken;
    }

    public async Task<Customer?> ValidateAndRevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = Hash(refreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (stored is null
            || stored.RevokedAt is not null
            || stored.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        await _refreshTokenRepository.RevokeAsync(stored.Id, cancellationToken);

        return await _customerRepository.GetByIdAsync(stored.CustomerId, cancellationToken);
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
