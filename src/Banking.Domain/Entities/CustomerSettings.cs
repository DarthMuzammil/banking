namespace Banking.Domain.Entities;

public class CustomerSettings
{
    public Guid CustomerId { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool NotifyTransactions { get; set; }
    public bool NotifySecurity { get; set; }
    public bool NotifyMarketing { get; set; }
    public DateTime UpdatedAt { get; set; }
}
