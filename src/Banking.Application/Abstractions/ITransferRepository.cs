using System.Data.Common;
using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface ITransferRepository
{
    Task InsertAsync(
        Transfer transfer,
        DbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default);
}
