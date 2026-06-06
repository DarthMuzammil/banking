using System.Data;
using System.Data.Common;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace Banking.Infrastructure.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public AuditLogRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AppendAsync(
        AuditLogEntry entry,
        DbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var ownsConnection = dbTransaction is null;
        var connection = ownsConnection
            ? await _connectionFactory.CreateOpenConnectionAsync(cancellationToken)
            : dbTransaction!.Connection;

        try
        {
            await using var command = ((SqlConnection)connection).CreateCommand();

            if (dbTransaction is not null)
            {
                command.Transaction = (SqlTransaction)dbTransaction;
            }

            command.CommandText = """
                INSERT INTO dbo.AuditLog (Id, CustomerId, Action, EntityType, EntityId, Details, CreatedAt)
                VALUES (@Id, @CustomerId, @Action, @EntityType, @EntityId, @Details, @CreatedAt)
                """;

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entry.Id });
            command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = (object?)entry.CustomerId ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@Action", SqlDbType.NVarChar, 100) { Value = entry.Action });
            command.Parameters.Add(new SqlParameter("@EntityType", SqlDbType.NVarChar, 50) { Value = entry.EntityType });
            command.Parameters.Add(new SqlParameter("@EntityId", SqlDbType.UniqueIdentifier) { Value = (object?)entry.EntityId ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@Details", SqlDbType.NVarChar, -1) { Value = (object?)entry.Details ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = entry.CreatedAt });

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            if (ownsConnection && connection is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }
    }
}
