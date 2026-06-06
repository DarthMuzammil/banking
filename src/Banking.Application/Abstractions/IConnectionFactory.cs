using System.Data.Common;

namespace Banking.Application.Abstractions;

public interface IConnectionFactory
{
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
