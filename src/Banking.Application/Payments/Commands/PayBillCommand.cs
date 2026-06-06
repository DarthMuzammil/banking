namespace Banking.Application.Payments.Commands;

public sealed record PayBillCommand(
    Guid CustomerId,
    Guid BillId,
    Guid AccountId);
