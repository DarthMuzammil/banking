using Banking.Domain.Enums;

namespace Banking.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public CustomerRole Role { get; set; } = CustomerRole.Customer;
    public DateTime CreatedAt { get; set; }
}
