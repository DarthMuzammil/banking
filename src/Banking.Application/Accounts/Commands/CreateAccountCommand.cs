using Banking.Domain.Enums;

namespace Banking.Application.Accounts.Commands;

public sealed record CreateAccountCommand(
    Guid CustomerId,
    AccountType AccountType);
