using Banking.Application.Abstractions;
using Banking.Application.Accounts.DTOs;
using Banking.Application.Common;
using Banking.Domain.Entities;
using Banking.Domain.Enums;

namespace Banking.Application.Accounts.Commands;

public sealed class CreateAccountCommandHandler
{
    private readonly IAccountRepository _accountRepository;

    public CreateAccountCommandHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result<CreateAccountResponseDto>> HandleAsync(
        CreateAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.AccountType is not AccountType.Checking and not AccountType.Savings)
        {
            return Result<CreateAccountResponseDto>.Failure(
                "Account type must be Checking or Savings.",
                "INVALID_ACCOUNT_TYPE");
        }

        var account = new Account
        {
            Id = Guid.NewGuid(),
            CustomerId = command.CustomerId,
            AccountNumber = GenerateAccountNumber(command.AccountType),
            AccountType = command.AccountType,
            Balance = 0,
            Currency = "USD",
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var accountId = await _accountRepository.CreateAsync(account, cancellationToken);

        return Result<CreateAccountResponseDto>.Success(new CreateAccountResponseDto(
            accountId,
            account.AccountNumber,
            account.AccountType,
            account.Balance,
            account.Currency));
    }

    private static string GenerateAccountNumber(AccountType accountType)
    {
        var prefix = accountType == AccountType.Checking ? "CHK" : "SAV";
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"{prefix}-{suffix}";
    }
}
