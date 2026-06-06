using System.Data;
using System.Data.Common;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Microsoft.Data.SqlClient;

namespace Banking.Infrastructure.Repositories;

public sealed class TransferRepository : ITransferRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public TransferRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(
        Transfer transfer,
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
                INSERT INTO dbo.Transfer
                    (Id, FromAccountId, ToAccountId, Amount, Status, Reference, CreatedAt)
                VALUES
                    (@Id, @FromAccountId, @ToAccountId, @Amount, @Status, @Reference, @CreatedAt)
                """;

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = transfer.Id });
            command.Parameters.Add(new SqlParameter("@FromAccountId", SqlDbType.UniqueIdentifier) { Value = transfer.FromAccountId });
            command.Parameters.Add(new SqlParameter("@ToAccountId", SqlDbType.UniqueIdentifier) { Value = transfer.ToAccountId });
            command.Parameters.Add(new SqlParameter("@Amount", SqlDbType.Decimal) { Value = transfer.Amount, Precision = 18, Scale = 2 });
            command.Parameters.Add(new SqlParameter("@Status", SqlDbType.TinyInt) { Value = (byte)transfer.Status });
            command.Parameters.Add(new SqlParameter("@Reference", SqlDbType.NVarChar, 50) { Value = transfer.Reference });
            command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = transfer.CreatedAt });

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
