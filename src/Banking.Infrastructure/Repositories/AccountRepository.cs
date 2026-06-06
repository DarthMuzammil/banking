using System.Data;
using System.Data.Common;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Banking.Domain.Exceptions;
using Microsoft.Data.SqlClient;

namespace Banking.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public AccountRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, CustomerId, AccountNumber, AccountType, Balance, Currency, Status, CreatedAt, RowVersion
            FROM dbo.Account
            WHERE CustomerId = @CustomerId
            ORDER BY CreatedAt
            """;

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customerId });

        var accounts = new List<Account>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            accounts.Add(MapAccount(reader));
        }

        return accounts;
    }

    public async Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, CustomerId, AccountNumber, AccountType, Balance, Currency, Status, CreatedAt, RowVersion
            FROM dbo.Account
            WHERE Id = @Id
            """;

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = accountId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapAccount(reader) : null;
    }

    public async Task<Guid> CreateAsync(Account account, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            INSERT INTO dbo.Account (Id, CustomerId, AccountNumber, AccountType, Balance, Currency, Status, CreatedAt)
            VALUES (@Id, @CustomerId, @AccountNumber, @AccountType, @Balance, @Currency, @Status, @CreatedAt)
            """;

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = account.Id });
        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = account.CustomerId });
        command.Parameters.Add(new SqlParameter("@AccountNumber", SqlDbType.NVarChar, 20) { Value = account.AccountNumber });
        command.Parameters.Add(new SqlParameter("@AccountType", SqlDbType.TinyInt) { Value = (byte)account.AccountType });
        command.Parameters.Add(new SqlParameter("@Balance", SqlDbType.Decimal) { Value = account.Balance, Precision = 18, Scale = 2 });
        command.Parameters.Add(new SqlParameter("@Currency", SqlDbType.Char, 3) { Value = account.Currency });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.TinyInt) { Value = (byte)account.Status });
        command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = account.CreatedAt });

        await command.ExecuteNonQueryAsync(cancellationToken);
        return account.Id;
    }
    public async Task UpdateBalanceAsync(
        Guid accountId,
        decimal newBalance,
        byte[] rowVersion,
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
            UPDATE dbo.Account
            SET Balance = @NewBalance
            WHERE Id = @AccountId
              AND RowVersion = @RowVersion
            """;

            command.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.UniqueIdentifier) { Value = accountId });
            command.Parameters.Add(new SqlParameter("@NewBalance", SqlDbType.Decimal) { Value = newBalance, Precision = 18, Scale = 2 });
            command.Parameters.Add(new SqlParameter("@RowVersion", SqlDbType.Timestamp) { Value = rowVersion });

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
            {
                throw new ConcurrencyException("Account", accountId);
            }
        }
        finally
        {
            if (ownsConnection && connection is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }
    }
    private static Account MapAccount(SqlDataReader reader) => new()
    {
        Id = reader.GetGuid(reader.GetOrdinal("Id")),
        CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
        AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber")),
        AccountType = (AccountType)reader.GetByte(reader.GetOrdinal("AccountType")),
        Balance = reader.GetDecimal(reader.GetOrdinal("Balance")),
        Currency = reader.GetString(reader.GetOrdinal("Currency")),
        Status = (AccountStatus)reader.GetByte(reader.GetOrdinal("Status")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
        RowVersion = (byte[])reader["RowVersion"]
    };
}