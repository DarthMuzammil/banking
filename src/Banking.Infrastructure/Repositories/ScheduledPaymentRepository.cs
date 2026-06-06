using System.Data;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Microsoft.Data.SqlClient;

namespace Banking.Infrastructure.Repositories;

public sealed class ScheduledPaymentRepository : IScheduledPaymentRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public ScheduledPaymentRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ScheduledPayment>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, CustomerId, AccountId, Payee, Amount, Frequency, NextDate
            FROM dbo.ScheduledPayment
            WHERE CustomerId = @CustomerId
            ORDER BY NextDate
            """;

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customerId });

        var items = new List<ScheduledPayment>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ScheduledPayment
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
                AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                Payee = reader.GetString(reader.GetOrdinal("Payee")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                Frequency = (PaymentFrequency)reader.GetByte(reader.GetOrdinal("Frequency")),
                NextDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("NextDate")))
            });
        }

        return items;
    }

    public async Task<Guid> CreateAsync(ScheduledPayment payment, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            INSERT INTO dbo.ScheduledPayment (Id, CustomerId, AccountId, Payee, Amount, Frequency, NextDate)
            VALUES (@Id, @CustomerId, @AccountId, @Payee, @Amount, @Frequency, @NextDate)
            """;

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = payment.Id });
        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = payment.CustomerId });
        command.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.UniqueIdentifier) { Value = payment.AccountId });
        command.Parameters.Add(new SqlParameter("@Payee", SqlDbType.NVarChar, 100) { Value = payment.Payee });
        command.Parameters.Add(new SqlParameter("@Amount", SqlDbType.Decimal) { Value = payment.Amount, Precision = 18, Scale = 2 });
        command.Parameters.Add(new SqlParameter("@Frequency", SqlDbType.TinyInt) { Value = (byte)payment.Frequency });
        command.Parameters.Add(new SqlParameter("@NextDate", SqlDbType.Date) { Value = payment.NextDate.ToDateTime(TimeOnly.MinValue) });

        await command.ExecuteNonQueryAsync(cancellationToken);
        return payment.Id;
    }
}
