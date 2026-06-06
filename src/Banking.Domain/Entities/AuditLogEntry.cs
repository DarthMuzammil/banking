namespace Banking.Domain.Entities;

public class AuditLogEntry
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
