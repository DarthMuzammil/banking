namespace Banking.Application.Transactions.Queries;

public sealed record GetCustomerTransactionsQuery(
    Guid CustomerId,
    Guid? AccountId,
    string? Search,
    int Skip,
    int Take);
