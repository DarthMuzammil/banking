namespace Banking.Application.Accounts.Queries;

public sealed record GetAccountByIdQuery(Guid CustomerId, Guid AccountId);