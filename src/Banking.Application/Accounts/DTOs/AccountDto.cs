using Banking.Domain.Enums;

namespace Banking.Application.Accounts.DTOs;

public sealed record AccountDto(
    Guid Id,
    string AccountNumber,
    AccountType AccountType,
    decimal Balance,
    string Currency,
    AccountStatus Status,
    DateTime CreatedAt);