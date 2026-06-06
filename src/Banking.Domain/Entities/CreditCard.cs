namespace Banking.Domain.Entities;

public class CreditCard
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastFour { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal CreditLimit { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal MinPayment { get; set; }
    public decimal Apr { get; set; }
}
