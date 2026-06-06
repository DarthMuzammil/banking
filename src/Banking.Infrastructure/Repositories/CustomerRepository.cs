using System.Data;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace Banking.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public CustomerRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, Email, FirstName, LastName, CreatedAt
            FROM dbo.Customer
            WHERE Email = @Email
            """;

        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = email });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapCustomer(reader) : null;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT Id, Email, FirstName, LastName, CreatedAt
            FROM dbo.Customer
            WHERE Id = @Id
            """;

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = id });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapCustomer(reader) : null;
    }

    public async Task<string?> GetPasswordHashAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = ((SqlConnection)connection).CreateCommand();

        command.CommandText = """
            SELECT PasswordHash
            FROM dbo.CustomerAuth
            WHERE CustomerId = @CustomerId
            """;

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customerId });

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? null : (string)result;
    }

    public async Task<Guid> CreateAsync(Customer customer, string passwordHash, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var sqlConnection = (SqlConnection)connection;
        await using var transaction = (SqlTransaction)await sqlConnection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using (var customerCommand = sqlConnection.CreateCommand())
            {
                customerCommand.Transaction = transaction;
                customerCommand.CommandText = """
                    INSERT INTO dbo.Customer (Id, Email, FirstName, LastName, CreatedAt)
                    VALUES (@Id, @Email, @FirstName, @LastName, @CreatedAt)
                    """;

                customerCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = customer.Id });
                customerCommand.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = customer.Email });
                customerCommand.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 100) { Value = customer.FirstName });
                customerCommand.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 100) { Value = customer.LastName });
                customerCommand.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = customer.CreatedAt });

                await customerCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var authCommand = sqlConnection.CreateCommand())
            {
                authCommand.Transaction = transaction;
                authCommand.CommandText = """
                    INSERT INTO dbo.CustomerAuth (CustomerId, PasswordHash, UpdatedAt)
                    VALUES (@CustomerId, @PasswordHash, @UpdatedAt)
                    """;

                authCommand.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customer.Id });
                authCommand.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 500) { Value = passwordHash });
                authCommand.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = DateTime.UtcNow });

                await authCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return customer.Id;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static Customer MapCustomer(SqlDataReader reader) => new()
    {
        Id = reader.GetGuid(reader.GetOrdinal("Id")),
        Email = reader.GetString(reader.GetOrdinal("Email")),
        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        LastName = reader.GetString(reader.GetOrdinal("LastName")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
    };
}
