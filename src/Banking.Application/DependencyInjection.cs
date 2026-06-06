using Banking.Application.Accounts.Commands;
using Banking.Application.Auth.Commands;
using Banking.Application.Auth.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterCustomerCommandHandler>();
        services.AddScoped<CreateAccountCommandHandler>();
        services.AddScoped<LoginQueryHandler>();
        return services;
    }
}
