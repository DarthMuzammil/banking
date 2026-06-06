using Banking.Application.Accounts.Commands;
using Banking.Application.Accounts.Queries;
using Banking.Application.Admin.Queries;
using Banking.Application.Auth.Commands;
using Banking.Application.Auth.Queries;
using Banking.Application.CreditCards.Queries;
using Banking.Application.Insights.Queries;
using Banking.Application.Investments.Queries;
using Banking.Application.Payments.Commands;
using Banking.Application.Payments.Queries;
using Banking.Application.Settings.Commands;
using Banking.Application.Settings.Queries;
using Banking.Application.Transactions.Commands;
using Banking.Application.Transactions.Queries;
using Banking.Application.Transfers.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterCustomerCommandHandler>();
        services.AddScoped<LoginQueryHandler>();
        services.AddScoped<RefreshTokenCommandHandler>();
        services.AddScoped<SearchCustomersQueryHandler>();
        services.AddScoped<GetCustomerAccountsAdminQueryHandler>();
        services.AddScoped<CreateAccountCommandHandler>();
        services.AddScoped<GetCustomerAccountsQueryHandler>();
        services.AddScoped<GetAccountByIdQueryHandler>();
        services.AddScoped<DepositCommandHandler>();
        services.AddScoped<WithdrawCommandHandler>();
        services.AddScoped<GetAccountTransactionsQueryHandler>();
        services.AddScoped<GetCustomerTransactionsQueryHandler>();
        services.AddScoped<TransferCommandHandler>();

        services.AddScoped<GetSettingsQueryHandler>();
        services.AddScoped<UpdateSettingsCommandHandler>();
        services.AddScoped<GetSpendingInsightsQueryHandler>();
        services.AddScoped<GetCreditCardsQueryHandler>();
        services.AddScoped<GetCardSpendingQueryHandler>();
        services.AddScoped<GetPortfolioQueryHandler>();
        services.AddScoped<GetBillPaymentsQueryHandler>();
        services.AddScoped<GetScheduledPaymentsQueryHandler>();
        services.AddScoped<PayBillCommandHandler>();

        return services;
    }
}
