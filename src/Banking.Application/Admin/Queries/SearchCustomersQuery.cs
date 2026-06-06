namespace Banking.Application.Admin.Queries;

public sealed record SearchCustomersQuery(string? Search, int Take = 20);
