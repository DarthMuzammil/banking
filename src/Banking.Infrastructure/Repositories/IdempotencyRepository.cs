using System.Data;
using Banking.Application.Abstractions;
using Microsoft.Data.SqlClient;

namespace Banking.Infrastructure.Repositories;

public sealed class IdempotencyRepository : IIdempotencyRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public IdempotencyRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IdempotencyCachedResponse?> GetAsync(
        Guid customerId,
        string idempotencyKey,
        string requestPath,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT ResponseStatus, ResponseBody
            FROM dbo.IdempotencyRecord
            WHERE CustomerId = @CustomerId
              AND IdempotencyKey = @IdempotencyKey
              AND RequestPath = @RequestPath
            """;

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customerId });
        command.Parameters.Add(new SqlParameter("@IdempotencyKey", SqlDbType.NVarChar, 100) { Value = idempotencyKey });
        command.Parameters.Add(new SqlParameter("@RequestPath", SqlDbType.NVarChar, 200) { Value = requestPath });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new IdempotencyCachedResponse(
            reader.GetInt32(reader.GetOrdinal("ResponseStatus")),
            reader.GetString(reader.GetOrdinal("ResponseBody")));
    }

    public async Task StoreAsync(
        Guid customerId,
        string idempotencyKey,
        string requestPath,
        int responseStatus,
        string responseBody,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            INSERT INTO dbo.IdempotencyRecord
                (Id, CustomerId, IdempotencyKey, RequestPath, ResponseStatus, ResponseBody, CreatedAt)
            VALUES
                (@Id, @CustomerId, @IdempotencyKey, @RequestPath, @ResponseStatus, @ResponseBody, @CreatedAt)
            """;

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customerId });
        command.Parameters.Add(new SqlParameter("@IdempotencyKey", SqlDbType.NVarChar, 100) { Value = idempotencyKey });
        command.Parameters.Add(new SqlParameter("@RequestPath", SqlDbType.NVarChar, 200) { Value = requestPath });
        command.Parameters.Add(new SqlParameter("@ResponseStatus", SqlDbType.Int) { Value = responseStatus });
        command.Parameters.Add(new SqlParameter("@ResponseBody", SqlDbType.NVarChar, -1) { Value = responseBody });
        command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = DateTime.UtcNow });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
