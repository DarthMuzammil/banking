namespace Banking.Application.Transactions.DTOs;

public sealed record WithdrawResponseDto(
    Guid TransactionId,
    Guid AccountId,
    decimal Amount,
    decimal BalanceAfter,
    string? Description,
    DateTime CreatedAt);