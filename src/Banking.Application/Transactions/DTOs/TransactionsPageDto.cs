namespace Banking.Application.Transactions.DTOs;

public sealed record TransactionsPageDto(
    Guid AccountId,
    int Skip,
    int Take,
    IReadOnlyList<TransactionDto> Items);