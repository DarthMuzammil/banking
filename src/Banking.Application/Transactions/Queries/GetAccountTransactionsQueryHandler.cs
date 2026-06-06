using Banking.Application.Abstractions;
using Banking.Application.Common;
using Banking.Application.Transactions.DTOs;

namespace Banking.Application.Transactions.Queries;

public sealed class GetAccountTransactionsQueryHandler
{
    private const int DefaultTake = 20;
    private const int MaxTake = 100;

    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetAccountTransactionsQueryHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<TransactionsPageDto>> HandleAsync(
        GetAccountTransactionsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Skip < 0)
        {
            return Result<TransactionsPageDto>.Failure(
                "Skip must be zero or greater.",
                "INVALID_SKIP");
        }

        var take = query.Take <= 0 ? DefaultTake : Math.Min(query.Take, MaxTake);

        var account = await _accountRepository.GetByIdAsync(query.AccountId, cancellationToken);
        if (account is null || account.CustomerId != query.CustomerId)
        {
            return Result<TransactionsPageDto>.Failure(
                "Account not found.",
                "ACCOUNT_NOT_FOUND");
        }

        var transactions = await _transactionRepository.GetByAccountIdAsync(
            query.AccountId,
            query.Skip,
            take,
            cancellationToken);

        var items = transactions
            .Select(t => new TransactionDto(
                t.Id,
                t.Type,
                t.Amount,
                t.BalanceAfter,
                t.Description,
                t.CreatedAt))
            .ToList();

        return Result<TransactionsPageDto>.Success(new TransactionsPageDto(
            query.AccountId,
            query.Skip,
            take,
            items));
    }
}