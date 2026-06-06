using Banking.Application.Abstractions;
using Banking.Application.Common;
using Banking.Application.Payments.DTOs;
using Banking.Domain.Entities;
using Banking.Domain.Enums;

namespace Banking.Application.Payments.Commands;

public sealed class PayBillCommandHandler
{
    private readonly IBillPaymentRepository _billPaymentRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPaymentGatewayService _paymentGateway;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IConnectionFactory _connectionFactory;

    public PayBillCommandHandler(
        IBillPaymentRepository billPaymentRepository,
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IPaymentGatewayService paymentGateway,
        IAuditLogRepository auditLogRepository,
        IConnectionFactory connectionFactory)
    {
        _billPaymentRepository = billPaymentRepository;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _paymentGateway = paymentGateway;
        _auditLogRepository = auditLogRepository;
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<PayBillResponseDto>> HandleAsync(
        PayBillCommand command,
        CancellationToken cancellationToken = default)
    {
        var bill = await _billPaymentRepository.GetByIdAsync(command.BillId, cancellationToken);
        if (bill is null || bill.CustomerId != command.CustomerId)
        {
            return Result<PayBillResponseDto>.Failure("Bill not found.", "BILL_NOT_FOUND");
        }

        if (bill.Status == BillStatus.Paid)
        {
            return Result<PayBillResponseDto>.Failure("Bill is already paid.", "BILL_ALREADY_PAID");
        }

        var account = await _accountRepository.GetByIdAsync(command.AccountId, cancellationToken);
        if (account is null || account.CustomerId != command.CustomerId)
        {
            return Result<PayBillResponseDto>.Failure("Account not found.", "ACCOUNT_NOT_FOUND");
        }

        if (account.Status != AccountStatus.Active)
        {
            return Result<PayBillResponseDto>.Failure("Account is not active.", "ACCOUNT_NOT_ACTIVE");
        }

        if (bill.Amount > account.Balance)
        {
            return Result<PayBillResponseDto>.Failure(
                "Insufficient funds to pay this bill.",
                "INSUFFICIENT_FUNDS");
        }

        var newBalance = account.Balance - bill.Amount;
        var transactionId = Guid.NewGuid();
        var paidAt = DateTime.UtcNow;
        var description = $"Bill payment — {bill.Payee}";

        var transaction = new Transaction
        {
            Id = transactionId,
            AccountId = account.Id,
            Type = TransactionType.Debit,
            Amount = bill.Amount,
            BalanceAfter = newBalance,
            Description = description,
            ReferenceId = bill.Id,
            Category = TransactionCategory.Bills,
            CreatedAt = paidAt
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

            await _transactionRepository.InsertAsync(transaction, dbTransaction, cancellationToken);

            await _billPaymentRepository.UpdateStatusAsync(
                bill.Id,
                BillStatus.Paid,
                paidAt,
                dbTransaction,
                cancellationToken);

            await AuditHelper.LogAsync(
                _auditLogRepository,
                command.CustomerId,
                "PayBill",
                "BillPayment",
                bill.Id,
                $"Payee {bill.Payee}; amount {bill.Amount:F2}; account {account.Id}",
                dbTransaction,
                cancellationToken);

            await dbTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }

        var reference = await _paymentGateway.ProcessBillPaymentAsync(
            bill.Payee,
            bill.Amount,
            cancellationToken);

        return Result<PayBillResponseDto>.Success(new PayBillResponseDto(
            bill.Id,
            transactionId,
            bill.Amount,
            reference,
            newBalance));
    }
}
