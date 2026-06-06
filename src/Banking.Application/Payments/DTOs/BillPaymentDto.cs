namespace Banking.Application.Payments.DTOs;

public sealed record BillPaymentDto(
    Guid Id,
    string Payee,
    decimal Amount,
    DateOnly DueDate,
    string Status,
    Guid? AccountId);

public sealed record ScheduledPaymentDto(
    Guid Id,
    string Payee,
    decimal Amount,
    string Frequency,
    DateOnly NextDate,
    Guid AccountId);

public sealed record PayBillResponseDto(
    Guid BillId,
    Guid TransactionId,
    decimal Amount,
    string Reference,
    decimal BalanceAfter);
