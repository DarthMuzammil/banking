namespace Banking.Application.Transactions.DTOs;

public sealed record DepositResponseDto(
    Guid TransactionId,
    Guid AccountId,
    decimal Amount,
    decimal BalanceAfter,
    string? Description,
    DateTime CreatedAt);