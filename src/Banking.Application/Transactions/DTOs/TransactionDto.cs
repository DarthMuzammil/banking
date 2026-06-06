using Banking.Domain.Enums;

namespace Banking.Application.Transactions.DTOs;

public sealed record TransactionDto(
    Guid Id,
    TransactionType Type,
    decimal Amount,
    decimal BalanceAfter,
    string? Description,
    DateTime CreatedAt);