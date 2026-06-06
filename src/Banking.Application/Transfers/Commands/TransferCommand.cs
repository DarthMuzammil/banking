namespace Banking.Application.Transfers.Commands;

public sealed record TransferCommand(
    Guid CustomerId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string? Description);
