using Banking.Domain.Enums;

namespace Banking.Application.Transactions.DTOs;

public sealed record CustomerTransactionDto(
    Guid Id,
    Guid AccountId,
    string AccountNumber,
    TransactionType Type,
    decimal Amount,
    decimal BalanceAfter,
    string? Description,
    DateTime CreatedAt);
