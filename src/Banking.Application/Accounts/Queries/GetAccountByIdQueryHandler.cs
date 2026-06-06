using Banking.Application.Abstractions;
using Banking.Application.Accounts.DTOs;
using Banking.Application.Common;
using Banking.Domain.Entities;

namespace Banking.Application.Accounts.Queries;

public sealed class GetAccountByIdQueryHandler
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountByIdQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result<AccountDto>> HandleAsync(
        GetAccountByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(query.AccountId, cancellationToken);
        
        if (account is null || account.CustomerId != query.CustomerId)
        {
            return Result<AccountDto>.Failure("Account not found.", "ACCOUNT_NOT_FOUND");
        }

        return Result<AccountDto>.Success(new AccountDto(
    account.Id,
    account.AccountNumber,
    account.AccountType,
    account.Balance,
    account.Currency,
    account.Status,
    account.CreatedAt));
    }
}