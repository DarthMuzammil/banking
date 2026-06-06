using System.Data.Common;
using Banking.Application.Insights;
using Banking.Application.Transactions;
using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface ITransactionRepository
{
    Task InsertAsync(
        Transaction transaction,
        DbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(
        Guid accountId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerTransactionRow>> GetByCustomerIdAsync(
        Guid customerId,
        Guid? accountId,
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategorySpendingRow>> GetDebitTotalsByCategoryAsync(
        Guid customerId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}
