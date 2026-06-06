using System.Data;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace Banking.Infrastructure.Repositories;

public sealed class InvestmentRepository : IInvestmentRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public InvestmentRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<InvestmentHolding>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, CustomerId, Symbol, Name, Shares, CurrentValue, DayChangePercent
            FROM dbo.InvestmentHolding
            WHERE CustomerId = @CustomerId
            ORDER BY Symbol
            """;

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customerId });

        var holdings = new List<InvestmentHolding>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            holdings.Add(new InvestmentHolding
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
                Symbol = reader.GetString(reader.GetOrdinal("Symbol")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Shares = reader.GetDecimal(reader.GetOrdinal("Shares")),
                CurrentValue = reader.GetDecimal(reader.GetOrdinal("CurrentValue")),
                DayChangePercent = reader.GetDecimal(reader.GetOrdinal("DayChangePercent"))
            });
        }

        return holdings;
    }
}
