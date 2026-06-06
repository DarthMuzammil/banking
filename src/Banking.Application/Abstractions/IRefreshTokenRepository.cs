using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface IRefreshTokenRepository
{
    Task CreateAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAsync(Guid tokenId, CancellationToken cancellationToken = default);
}
