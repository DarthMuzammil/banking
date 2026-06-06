using System.Data;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace Banking.Infrastructure.Repositories;

public sealed class CustomerSettingsRepository : ICustomerSettingsRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public CustomerSettingsRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CustomerSettings?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT CustomerId, TwoFactorEnabled, NotifyTransactions, NotifySecurity, NotifyMarketing, UpdatedAt
            FROM dbo.CustomerSettings
            WHERE CustomerId = @CustomerId
            """;

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customerId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task UpsertAsync(CustomerSettings settings, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            MERGE dbo.CustomerSettings AS target
            USING (SELECT @CustomerId AS CustomerId) AS source
            ON target.CustomerId = source.CustomerId
            WHEN MATCHED THEN
                UPDATE SET
                    TwoFactorEnabled = @TwoFactorEnabled,
                    NotifyTransactions = @NotifyTransactions,
                    NotifySecurity = @NotifySecurity,
                    NotifyMarketing = @NotifyMarketing,
                    UpdatedAt = @UpdatedAt
            WHEN NOT MATCHED THEN
                INSERT (CustomerId, TwoFactorEnabled, NotifyTransactions, NotifySecurity, NotifyMarketing, UpdatedAt)
                VALUES (@CustomerId, @TwoFactorEnabled, @NotifyTransactions, @NotifySecurity, @NotifyMarketing, @UpdatedAt);
            """;

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = settings.CustomerId });
        command.Parameters.Add(new SqlParameter("@TwoFactorEnabled", SqlDbType.Bit) { Value = settings.TwoFactorEnabled });
        command.Parameters.Add(new SqlParameter("@NotifyTransactions", SqlDbType.Bit) { Value = settings.NotifyTransactions });
        command.Parameters.Add(new SqlParameter("@NotifySecurity", SqlDbType.Bit) { Value = settings.NotifySecurity });
        command.Parameters.Add(new SqlParameter("@NotifyMarketing", SqlDbType.Bit) { Value = settings.NotifyMarketing });
        command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = settings.UpdatedAt });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static CustomerSettings Map(SqlDataReader reader) => new()
    {
        CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
        TwoFactorEnabled = reader.GetBoolean(reader.GetOrdinal("TwoFactorEnabled")),
        NotifyTransactions = reader.GetBoolean(reader.GetOrdinal("NotifyTransactions")),
        NotifySecurity = reader.GetBoolean(reader.GetOrdinal("NotifySecurity")),
        NotifyMarketing = reader.GetBoolean(reader.GetOrdinal("NotifyMarketing")),
        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
    };
}
