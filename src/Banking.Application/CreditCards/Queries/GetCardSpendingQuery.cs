namespace Banking.Application.CreditCards.Queries;

public sealed record GetCardSpendingQuery(Guid CustomerId, Guid CardId);
