namespace Banking.Application.Transactions.DTOs;

public sealed record CustomerTransactionsPageDto(
    int Skip,
    int Take,
    IReadOnlyList<CustomerTransactionDto> Items);
