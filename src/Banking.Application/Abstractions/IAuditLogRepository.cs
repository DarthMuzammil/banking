using System.Data.Common;
using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface IAuditLogRepository
{
    Task AppendAsync(
        AuditLogEntry entry,
        DbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default);
}
