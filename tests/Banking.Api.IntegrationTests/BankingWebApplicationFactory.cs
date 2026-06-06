using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Banking.Api.IntegrationTests;

public sealed class BankingWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:BankingDb"] =
                    Environment.GetEnvironmentVariable("BANKING_TEST_CONNECTION")
                    ?? "Server=localhost,1433;Database=BankingDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;",
                ["Jwt:Key"] = "dev-only-secret-key-min-32-chars-long!",
                ["Jwt:Issuer"] = "BankingApi",
                ["Jwt:Audience"] = "BankingWeb",
                ["Jwt:ExpiryMinutes"] = "60"
            });
        });
    }
}
