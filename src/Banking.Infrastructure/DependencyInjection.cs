using Banking.Application.Abstractions;
using Banking.Infrastructure.Data;
using Banking.Infrastructure.Repositories;
using Banking.Infrastructure.Security;
using Banking.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<IConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddSingleton<IEmailService, MockEmailService>();
        services.AddSingleton<IPaymentGatewayService, MockPaymentGatewayService>();

        services.AddScoped<ICustomerSettingsRepository, CustomerSettingsRepository>();
        services.AddScoped<ICreditCardRepository, CreditCardRepository>();
        services.AddScoped<IInvestmentRepository, InvestmentRepository>();
        services.AddScoped<IBillPaymentRepository, BillPaymentRepository>();
        services.AddScoped<IScheduledPaymentRepository, ScheduledPaymentRepository>();

        return services;
    }
}
