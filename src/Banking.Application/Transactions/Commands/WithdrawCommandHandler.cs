using Banking.Application.Abstractions;
using Banking.Application.Common;
using Banking.Application.Transactions.DTOs;
using Banking.Domain.Entities;
using Banking.Domain.Enums;

namespace Banking.Application.Transactions.Commands;

public sealed class WithdrawCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IConnectionFactory _connectionFactory;

    public WithdrawCommandHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IAuditLogRepository auditLogRepository,
        IConnectionFactory connectionFactory)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _auditLogRepository = auditLogRepository;
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<WithdrawResponseDto>> HandleAsync(
        WithdrawCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Amount <= 0)
        {
            return Result<WithdrawResponseDto>.Failure(
                "Withdrawal amount must be greater than zero.",
                "INVALID_AMOUNT");
        }

        var account = await _accountRepository.GetByIdAsync(command.AccountId, cancellationToken);
        if (account is null || account.CustomerId != command.CustomerId)
        {
            return Result<WithdrawResponseDto>.Failure(
                "Account not found.",
                "ACCOUNT_NOT_FOUND");
        }

        if (account.Status != AccountStatus.Active)
        {
            return Result<WithdrawResponseDto>.Failure(
                "Account is not active.",
                "ACCOUNT_NOT_ACTIVE");
        }

        if (command.Amount > account.Balance)
        {
            return Result<WithdrawResponseDto>.Failure(
                "Insufficient funds for this withdrawal.",
                "INSUFFICIENT_FUNDS");
        }

        var newBalance = account.Balance - command.Amount;
        var transactionId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var transactionEntity = new Transaction
        {
            Id = transactionId,
            AccountId = account.Id,
            Type = TransactionType.Debit,
            Amount = command.Amount,
            BalanceAfter = newBalance,
            Description = string.IsNullOrWhiteSpace(command.Description)
                ? "Withdrawal"
                : command.Description.Trim(),
            ReferenceId = null,
            Category = TransactionCategoryInferrer.Infer(command.Description, TransactionType.Debit),
            CreatedAt = createdAt
        };

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var dbTransaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await _accountRepository.UpdateBalanceAsync(
                account.Id,
                newBalance,
                account.RowVersion,
                dbTransaction,
                cancellationToken);

            await _transactionRepository.InsertAsync(
                transactionEntity,
                dbTransaction,
                cancellationToken);

            await AuditHelper.LogAsync(
                _auditLogRepository,
                command.CustomerId,
                "Withdraw",
                "Transaction",
                transactionId,
                $"Account {account.Id}; amount {command.Amount:F2}",
                dbTransaction,
                cancellationToken);

            await dbTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }

        return Result<WithdrawResponseDto>.Success(new WithdrawResponseDto(
            transactionId,
            account.Id,
            command.Amount,
            newBalance,
            transactionEntity.Description,
            createdAt));
    }
}