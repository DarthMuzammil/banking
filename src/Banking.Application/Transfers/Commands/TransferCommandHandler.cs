using Banking.Application.Abstractions;
using Banking.Application.Common;
using Banking.Application.Transfers.DTOs;
using Banking.Domain.Entities;
using Banking.Domain.Enums;

namespace Banking.Application.Transfers.Commands;

public sealed class TransferCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmailService _emailService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IConnectionFactory _connectionFactory;

    public TransferCommandHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ITransferRepository transferRepository,
        ICustomerRepository customerRepository,
        IEmailService emailService,
        IAuditLogRepository auditLogRepository,
        IConnectionFactory connectionFactory)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _transferRepository = transferRepository;
        _customerRepository = customerRepository;
        _emailService = emailService;
        _auditLogRepository = auditLogRepository;
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<TransferResponseDto>> HandleAsync(
        TransferCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Amount <= 0)
        {
            return Result<TransferResponseDto>.Failure(
                "Transfer amount must be greater than zero.",
                "INVALID_AMOUNT");
        }

        if (command.FromAccountId == command.ToAccountId)
        {
            return Result<TransferResponseDto>.Failure(
                "Cannot transfer to the same account.",
                "SAME_ACCOUNT");
        }

        var fromAccount = await _accountRepository.GetByIdAsync(command.FromAccountId, cancellationToken);
        if (fromAccount is null || fromAccount.CustomerId != command.CustomerId)
        {
            return Result<TransferResponseDto>.Failure(
                "Source account not found.",
                "ACCOUNT_NOT_FOUND");
        }

        var toAccount = await _accountRepository.GetByIdAsync(command.ToAccountId, cancellationToken);
        if (toAccount is null || toAccount.CustomerId != command.CustomerId)
        {
            return Result<TransferResponseDto>.Failure(
                "Destination account not found.",
                "ACCOUNT_NOT_FOUND");
        }

        if (fromAccount.Status != AccountStatus.Active || toAccount.Status != AccountStatus.Active)
        {
            return Result<TransferResponseDto>.Failure(
                "One or both accounts are not active.",
                "ACCOUNT_NOT_ACTIVE");
        }

        if (command.Amount > fromAccount.Balance)
        {
            return Result<TransferResponseDto>.Failure(
                "Insufficient funds for this transfer.",
                "INSUFFICIENT_FUNDS");
        }

        var transferId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var reference = $"TRF-{createdAt:yyyyMMdd}-{transferId.ToString("N")[..8].ToUpperInvariant()}";

        var fromNewBalance = fromAccount.Balance - command.Amount;
        var toNewBalance = toAccount.Balance + command.Amount;
        var description = string.IsNullOrWhiteSpace(command.Description)
            ? $"Transfer {reference}"
            : command.Description.Trim();

        var debitTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = fromAccount.Id,
            Type = TransactionType.Debit,
            Amount = command.Amount,
            BalanceAfter = fromNewBalance,
            Description = $"Transfer out — {description}",
            ReferenceId = transferId,
            Category = TransactionCategory.Transfer,
            CreatedAt = createdAt
        };

        var creditTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = toAccount.Id,
            Type = TransactionType.Credit,
            Amount = command.Amount,
            BalanceAfter = toNewBalance,
            Description = $"Transfer in — {description}",
            ReferenceId = transferId,
            Category = TransactionCategory.Transfer,
            CreatedAt = createdAt
        };

        var transferEntity = new Transfer
        {
            Id = transferId,
            FromAccountId = fromAccount.Id,
            ToAccountId = toAccount.Id,
            Amount = command.Amount,
            Status = TransferStatus.Completed,
            Reference = reference,
            CreatedAt = createdAt
        };

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var dbTransaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await _accountRepository.UpdateBalanceAsync(
                fromAccount.Id,
                fromNewBalance,
                fromAccount.RowVersion,
                dbTransaction,
                cancellationToken);

            await _accountRepository.UpdateBalanceAsync(
                toAccount.Id,
                toNewBalance,
                toAccount.RowVersion,
                dbTransaction,
                cancellationToken);

            await _transactionRepository.InsertAsync(debitTransaction, dbTransaction, cancellationToken);
            await _transactionRepository.InsertAsync(creditTransaction, dbTransaction, cancellationToken);
            await _transferRepository.InsertAsync(transferEntity, dbTransaction, cancellationToken);

            await AuditHelper.LogAsync(
                _auditLogRepository,
                command.CustomerId,
                "Transfer",
                "Transfer",
                transferId,
                $"From {fromAccount.Id} to {toAccount.Id}; amount {command.Amount:F2}; ref {reference}",
                dbTransaction,
                cancellationToken);

            await dbTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }

        var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);
        if (customer is not null)
        {
            await _emailService.SendTransferReceiptAsync(
                customer.Email,
                reference,
                command.Amount,
                cancellationToken);
        }

        return Result<TransferResponseDto>.Success(new TransferResponseDto(
            transferId,
            reference,
            fromAccount.Id,
            toAccount.Id,
            command.Amount,
            createdAt));
    }
}
