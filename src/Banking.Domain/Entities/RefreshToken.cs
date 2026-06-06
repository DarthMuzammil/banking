namespace Banking.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
