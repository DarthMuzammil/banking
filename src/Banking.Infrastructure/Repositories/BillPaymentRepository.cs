using System.Data;
using System.Data.Common;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Microsoft.Data.SqlClient;

namespace Banking.Infrastructure.Repositories;

public sealed class BillPaymentRepository : IBillPaymentRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public BillPaymentRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<BillPayment>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, CustomerId, AccountId, Payee, Amount, DueDate, Status, PaidAt
            FROM dbo.BillPayment
            WHERE CustomerId = @CustomerId
            ORDER BY DueDate
            """;

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customerId });

        var bills = new List<BillPayment>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            bills.Add(Map(reader));
        }

        return bills;
    }

    public async Task<BillPayment?> GetByIdAsync(Guid billId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, CustomerId, AccountId, Payee, Amount, DueDate, Status, PaidAt
            FROM dbo.BillPayment
            WHERE Id = @Id
            """;

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = billId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task UpdateStatusAsync(
        Guid billId,
        BillStatus status,
        DateTime? paidAt,
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
                UPDATE dbo.BillPayment
                SET Status = @Status, PaidAt = @PaidAt
                WHERE Id = @Id
                """;

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = billId });
            command.Parameters.Add(new SqlParameter("@Status", SqlDbType.TinyInt) { Value = (byte)status });
            command.Parameters.Add(new SqlParameter("@PaidAt", SqlDbType.DateTime2)
            {
                Value = paidAt.HasValue ? paidAt.Value : DBNull.Value
            });

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

    private static BillPayment Map(SqlDataReader reader) => new()
    {
        Id = reader.GetGuid(reader.GetOrdinal("Id")),
        CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
        AccountId = reader.IsDBNull(reader.GetOrdinal("AccountId"))
            ? null
            : reader.GetGuid(reader.GetOrdinal("AccountId")),
        Payee = reader.GetString(reader.GetOrdinal("Payee")),
        Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
        DueDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DueDate"))),
        Status = (BillStatus)reader.GetByte(reader.GetOrdinal("Status")),
        PaidAt = reader.IsDBNull(reader.GetOrdinal("PaidAt"))
            ? null
            : reader.GetDateTime(reader.GetOrdinal("PaidAt"))
    };
}
