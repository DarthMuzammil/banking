using System.Data;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace Banking.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public RefreshTokenRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            INSERT INTO dbo.RefreshToken (Id, CustomerId, TokenHash, ExpiresAt, CreatedAt, RevokedAt)
            VALUES (@Id, @CustomerId, @TokenHash, @ExpiresAt, @CreatedAt, @RevokedAt)
            """;

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = token.Id });
        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = token.CustomerId });
        command.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.NVarChar, 500) { Value = token.TokenHash });
        command.Parameters.Add(new SqlParameter("@ExpiresAt", SqlDbType.DateTime2) { Value = token.ExpiresAt });
        command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = token.CreatedAt });
        command.Parameters.Add(new SqlParameter("@RevokedAt", SqlDbType.DateTime2) { Value = (object?)token.RevokedAt ?? DBNull.Value });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, CustomerId, TokenHash, ExpiresAt, CreatedAt, RevokedAt
            FROM dbo.RefreshToken
            WHERE TokenHash = @TokenHash
            """;

        command.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.NVarChar, 500) { Value = tokenHash });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new RefreshToken
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
            TokenHash = reader.GetString(reader.GetOrdinal("TokenHash")),
            ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            RevokedAt = reader.IsDBNull(reader.GetOrdinal("RevokedAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("RevokedAt"))
        };
    }

    public async Task RevokeAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            UPDATE dbo.RefreshToken
            SET RevokedAt = @RevokedAt
            WHERE Id = @Id AND RevokedAt IS NULL
            """;

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = tokenId });
        command.Parameters.Add(new SqlParameter("@RevokedAt", SqlDbType.DateTime2) { Value = DateTime.UtcNow });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
