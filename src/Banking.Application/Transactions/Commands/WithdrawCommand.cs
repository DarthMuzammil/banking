namespace Banking.Application.Transactions.Commands;

public sealed record WithdrawCommand(
    Guid CustomerId,
    Guid AccountId,
    decimal Amount,
    string? Description);