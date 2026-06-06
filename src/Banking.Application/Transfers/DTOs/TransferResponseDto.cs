namespace Banking.Application.Transfers.DTOs;

public sealed record TransferResponseDto(
    Guid TransferId,
    string Reference,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    DateTime CreatedAt);
