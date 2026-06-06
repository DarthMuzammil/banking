using System.Data.Common;
using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface IBillPaymentRepository
{
    Task<IReadOnlyList<BillPayment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<BillPayment?> GetByIdAsync(Guid billId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(
        Guid billId,
        Domain.Enums.BillStatus status,
        DateTime? paidAt,
        DbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default);
}
