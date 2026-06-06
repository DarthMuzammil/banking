namespace Banking.Application.Transactions.Queries;

public sealed record GetAccountTransactionsQuery(
    Guid CustomerId,
    Guid AccountId,
    int Skip,
    int Take);