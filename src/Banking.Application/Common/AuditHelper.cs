using System.Data.Common;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;

namespace Banking.Application.Common;

public static class AuditHelper
{
    public static Task LogAsync(
        IAuditLogRepository repository,
        Guid customerId,
        string action,
        string entityType,
        Guid? entityId,
        string? details,
        DbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        return repository.AppendAsync(new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        }, dbTransaction, cancellationToken);
    }
}
