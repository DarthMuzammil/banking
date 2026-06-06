using Banking.Application.Abstractions;
using Banking.Application.Accounts.DTOs;
using Banking.Application.Common;
using Banking.Domain.Entities;

namespace Banking.Application.Accounts.Queries;

public sealed class GetCustomerAccountsQueryHandler
{
    private readonly IAccountRepository _accountRepository;

    public GetCustomerAccountsQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result<IReadOnlyList<AccountDto>>> HandleAsync(
        GetCustomerAccountsQuery query,
        CancellationToken cancellationToken = default)
    {
        var accounts = await _accountRepository.GetByCustomerIdAsync(
    query.CustomerId,
    cancellationToken);

        var dtos = accounts
        .Select(a => new AccountDto(
            a.Id,
            a.AccountNumber,
            a.AccountType,
            a.Balance,
            a.Currency,
            a.Status,
            a.CreatedAt))
        .ToList();
        
        return Result<IReadOnlyList<AccountDto>>.Success(dtos);
    }
}