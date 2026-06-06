using Banking.Domain.Enums;

namespace Banking.Domain.Entities;

public class ScheduledPayment
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid AccountId { get; set; }
    public string Payee { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentFrequency Frequency { get; set; }
    public DateOnly NextDate { get; set; }
}
