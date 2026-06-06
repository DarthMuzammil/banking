using Banking.Application.Abstractions;
using Banking.Application.Admin.DTOs;
using Banking.Application.Common;
using Banking.Domain.Enums;

namespace Banking.Application.Admin.Queries;

public sealed class GetCustomerAccountsAdminQueryHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAccountRepository _accountRepository;

    public GetCustomerAccountsAdminQueryHandler(
        ICustomerRepository customerRepository,
        IAccountRepository accountRepository)
    {
        _customerRepository = customerRepository;
        _accountRepository = accountRepository;
    }

    public async Task<Result<IReadOnlyList<AdminAccountDto>>> HandleAsync(
        GetCustomerAccountsAdminQuery query,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(query.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result<IReadOnlyList<AdminAccountDto>>.Failure(
                "Customer not found.",
                "CUSTOMER_NOT_FOUND");
        }

        var accounts = await _accountRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);

        var items = accounts
            .Select(a => new AdminAccountDto(
                a.Id,
                a.AccountNumber,
                MapAccountType(a.AccountType),
                a.Balance,
                a.Currency,
                MapStatus(a.Status)))
            .ToList();

        return Result<IReadOnlyList<AdminAccountDto>>.Success(items);
    }

    private static string MapAccountType(AccountType type) =>
        type == AccountType.Checking ? "Checking" : "Savings";

    private static string MapStatus(AccountStatus status) =>
        status == AccountStatus.Active ? "Active" : "Closed";
}
