using Banking.Domain.Enums;

namespace Banking.Application.Accounts.DTOs;

public sealed record CreateAccountResponseDto(
    Guid AccountId,
    string AccountNumber,
    AccountType AccountType,
    decimal Balance,
    string Currency);