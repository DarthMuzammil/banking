using System.Data;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace Banking.Infrastructure.Repositories;

public sealed class CreditCardRepository : ICreditCardRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public CreditCardRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CreditCard>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, CustomerId, Name, LastFour, Balance, CreditLimit, DueDate, MinPayment, Apr
            FROM dbo.CreditCard
            WHERE CustomerId = @CustomerId
            ORDER BY Name
            """;

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customerId });

        var cards = new List<CreditCard>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            cards.Add(MapCard(reader));
        }

        return cards;
    }

    public async Task<CreditCard?> GetByIdAsync(Guid cardId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, CustomerId, Name, LastFour, Balance, CreditLimit, DueDate, MinPayment, Apr
            FROM dbo.CreditCard
            WHERE Id = @Id
            """;

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = cardId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapCard(reader) : null;
    }

    public async Task<IReadOnlyList<CreditCardSpending>> GetSpendingByCardIdAsync(
        Guid cardId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, CardId, Category, Amount
            FROM dbo.CreditCardSpending
            WHERE CardId = @CardId
            ORDER BY Amount DESC
            """;

        command.Parameters.Add(new SqlParameter("@CardId", SqlDbType.UniqueIdentifier) { Value = cardId });

        var rows = new List<CreditCardSpending>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new CreditCardSpending
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                CardId = reader.GetGuid(reader.GetOrdinal("CardId")),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount"))
            });
        }

        return rows;
    }

    private static CreditCard MapCard(SqlDataReader reader) => new()
    {
        Id = reader.GetGuid(reader.GetOrdinal("Id")),
        CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        LastFour = reader.GetString(reader.GetOrdinal("LastFour")),
        Balance = reader.GetDecimal(reader.GetOrdinal("Balance")),
        CreditLimit = reader.GetDecimal(reader.GetOrdinal("CreditLimit")),
        DueDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DueDate"))),
        MinPayment = reader.GetDecimal(reader.GetOrdinal("MinPayment")),
        Apr = reader.GetDecimal(reader.GetOrdinal("Apr"))
    };
}
