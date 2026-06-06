namespace Banking.Application.Admin.DTOs;

public sealed record AdminCustomerDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime CreatedAt);

public sealed record AdminAccountDto(
    Guid Id,
    string AccountNumber,
    string AccountType,
    decimal Balance,
    string Currency,
    string Status);
