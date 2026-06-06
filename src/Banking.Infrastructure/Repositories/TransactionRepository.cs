using System.Data;
using System.Data.Common;
using Banking.Application.Abstractions;
using Banking.Application.Insights;
using Banking.Application.Transactions;
using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Microsoft.Data.SqlClient;

namespace Banking.Infrastructure.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public TransactionRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(
        Transaction transaction,
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
                INSERT INTO dbo.[Transaction]
                    (Id, AccountId, Type, Amount, BalanceAfter, Description, ReferenceId, Category, CreatedAt)
                VALUES
                    (@Id, @AccountId, @Type, @Amount, @BalanceAfter, @Description, @ReferenceId, @Category, @CreatedAt)
                """;

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = transaction.Id });
            command.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.UniqueIdentifier) { Value = transaction.AccountId });
            command.Parameters.Add(new SqlParameter("@Type", SqlDbType.TinyInt) { Value = (byte)transaction.Type });
            command.Parameters.Add(new SqlParameter("@Amount", SqlDbType.Decimal) { Value = transaction.Amount, Precision = 18, Scale = 2 });
            command.Parameters.Add(new SqlParameter("@BalanceAfter", SqlDbType.Decimal) { Value = transaction.BalanceAfter, Precision = 18, Scale = 2 });
            command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 500) { Value = (object?)transaction.Description ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@ReferenceId", SqlDbType.UniqueIdentifier) { Value = (object?)transaction.ReferenceId ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@Category", SqlDbType.TinyInt)
            {
                Value = transaction.Category.HasValue ? (byte)transaction.Category.Value : DBNull.Value
            });
            command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = transaction.CreatedAt });

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

    public async Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(
        Guid accountId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, AccountId, Type, Amount, BalanceAfter, Description, ReferenceId, Category, CreatedAt
            FROM dbo.[Transaction]
            WHERE AccountId = @AccountId
            ORDER BY CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;

        command.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.UniqueIdentifier) { Value = accountId });
        command.Parameters.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = skip });
        command.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = take });

        var transactions = new List<Transaction>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            transactions.Add(MapTransaction(reader));
        }

        return transactions;
    }

    public async Task<IReadOnlyList<CustomerTransactionRow>> GetByCustomerIdAsync(
        Guid customerId,
        Guid? accountId,
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT
                t.Id,
                t.AccountId,
                t.Type,
                t.Amount,
                t.BalanceAfter,
                t.Description,
                t.ReferenceId,
                t.Category,
                t.CreatedAt,
                a.AccountNumber
            FROM dbo.[Transaction] t
            INNER JOIN dbo.Account a ON t.AccountId = a.Id
            WHERE a.CustomerId = @CustomerId
              AND (@AccountId IS NULL OR t.AccountId = @AccountId)
              AND (@Search IS NULL OR t.Description LIKE '%' + @Search + '%')
            ORDER BY t.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customerId });
        command.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.UniqueIdentifier)
        {
            Value = accountId.HasValue ? accountId.Value : DBNull.Value
        });
        command.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar, 500)
        {
            Value = string.IsNullOrWhiteSpace(search) ? DBNull.Value : search.Trim()
        });
        command.Parameters.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = skip });
        command.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = take });

        var rows = new List<CustomerTransactionRow>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new CustomerTransactionRow(
                new Transaction
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                    Type = (TransactionType)reader.GetByte(reader.GetOrdinal("Type")),
                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                    BalanceAfter = reader.GetDecimal(reader.GetOrdinal("BalanceAfter")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Description")),
                    ReferenceId = reader.IsDBNull(reader.GetOrdinal("ReferenceId"))
                        ? null
                        : reader.GetGuid(reader.GetOrdinal("ReferenceId")),
                    Category = reader.IsDBNull(reader.GetOrdinal("Category"))
                        ? null
                        : (TransactionCategory)reader.GetByte(reader.GetOrdinal("Category")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                },
                reader.GetString(reader.GetOrdinal("AccountNumber"))));
        }

        return rows;
    }

    public async Task<IReadOnlyList<CategorySpendingRow>> GetDebitTotalsByCategoryAsync(
        Guid customerId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT COALESCE(t.Category, 7) AS Category, SUM(t.Amount) AS Total
            FROM dbo.[Transaction] t
            INNER JOIN dbo.Account a ON t.AccountId = a.Id
            WHERE a.CustomerId = @CustomerId
              AND t.Type = 2
              AND t.CreatedAt >= @FromUtc
              AND t.CreatedAt < @ToUtc
            GROUP BY COALESCE(t.Category, 7)
            """;

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customerId });
        command.Parameters.Add(new SqlParameter("@FromUtc", SqlDbType.DateTime2) { Value = fromUtc });
        command.Parameters.Add(new SqlParameter("@ToUtc", SqlDbType.DateTime2) { Value = toUtc });

        var rows = new List<CategorySpendingRow>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new CategorySpendingRow(
                (TransactionCategory)reader.GetInt32(reader.GetOrdinal("Category")),
                reader.GetDecimal(reader.GetOrdinal("Total"))));
        }

        return rows;
    }

    private static Transaction MapTransaction(SqlDataReader reader) => new()
    {
        Id = reader.GetGuid(reader.GetOrdinal("Id")),
        AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
        Type = (TransactionType)reader.GetByte(reader.GetOrdinal("Type")),
        Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
        BalanceAfter = reader.GetDecimal(reader.GetOrdinal("BalanceAfter")),
        Description = reader.IsDBNull(reader.GetOrdinal("Description"))
            ? null
            : reader.GetString(reader.GetOrdinal("Description")),
        ReferenceId = reader.IsDBNull(reader.GetOrdinal("ReferenceId"))
            ? null
            : reader.GetGuid(reader.GetOrdinal("ReferenceId")),
        Category = reader.IsDBNull(reader.GetOrdinal("Category"))
            ? null
            : (TransactionCategory)reader.GetByte(reader.GetOrdinal("Category")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
    };
}
