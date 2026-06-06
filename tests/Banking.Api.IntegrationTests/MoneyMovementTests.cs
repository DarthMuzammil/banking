using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Banking.Api.IntegrationTests;

public sealed class MoneyMovementTests : IClassFixture<BankingWebApplicationFactory>
{
    private static readonly Guid CheckingAccountId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SavingsAccountId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly HttpClient _client;

    public MoneyMovementTests(BankingWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Deposit_IncreasesBalance()
    {
        var token = await LoginAsync();
        using var request = CreateAuthRequest(HttpMethod.Post, $"/api/accounts/{CheckingAccountId}/deposits", token);
        request.Content = JsonContent.Create(new { amount = 1.00m, description = "Integration test deposit" });

        var response = await _client.SendAsync(request);

        Assert.True(
            response.StatusCode is not HttpStatusCode.InternalServerError,
            "SQL Server not available — start banking-sql and run init-db.sql");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("balanceAfter").GetDecimal() > 0);
    }

    [Fact]
    public async Task Withdraw_WithInsufficientFunds_ReturnsBadRequest()
    {
        var token = await LoginAsync();
        using var request = CreateAuthRequest(HttpMethod.Post, $"/api/accounts/{CheckingAccountId}/withdrawals", token);
        request.Content = JsonContent.Create(new { amount = 9_999_999.99m, description = "Too much" });

        var response = await _client.SendAsync(request);

        Assert.True(
            response.StatusCode is not HttpStatusCode.InternalServerError,
            "SQL Server not available — start banking-sql and run init-db.sql");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("INSUFFICIENT_FUNDS", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Transfer_WithSameIdempotencyKey_ReturnsCachedResponse()
    {
        var token = await LoginAsync();
        var idempotencyKey = Guid.NewGuid().ToString();

        using var request1 = CreateAuthRequest(HttpMethod.Post, "/api/transfers", token);
        request1.Headers.Add("Idempotency-Key", idempotencyKey);
        request1.Content = JsonContent.Create(new
        {
            fromAccountId = CheckingAccountId,
            toAccountId = SavingsAccountId,
            amount = 0.50m,
            description = "Idempotency test"
        });

        var response1 = await _client.SendAsync(request1);

        Assert.True(
            response1.StatusCode is not HttpStatusCode.InternalServerError,
            "SQL Server not available — start banking-sql and run init-db.sql");

        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var body1 = await response1.Content.ReadFromJsonAsync<JsonElement>();
        var reference1 = body1.GetProperty("reference").GetString();

        using var request2 = CreateAuthRequest(HttpMethod.Post, "/api/transfers", token);
        request2.Headers.Add("Idempotency-Key", idempotencyKey);
        request2.Content = JsonContent.Create(new
        {
            fromAccountId = CheckingAccountId,
            toAccountId = SavingsAccountId,
            amount = 0.50m,
            description = "Idempotency test"
        });

        var response2 = await _client.SendAsync(request2);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var body2 = await response2.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(reference1, body2.GetProperty("reference").GetString());
    }

    [Fact]
    public async Task Transfer_BetweenOwnAccounts_Succeeds()
    {
        var token = await LoginAsync();
        using var request = CreateAuthRequest(HttpMethod.Post, "/api/transfers", token);
        request.Content = JsonContent.Create(new
        {
            fromAccountId = CheckingAccountId,
            toAccountId = SavingsAccountId,
            amount = 1.00m,
            description = "Integration test transfer"
        });

        var response = await _client.SendAsync(request);

        Assert.True(
            response.StatusCode is not HttpStatusCode.InternalServerError,
            "SQL Server not available — start banking-sql and run init-db.sql");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("reference").GetString()));
    }

    private async Task<string> LoginAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "demo@bank.local",
            password = "Demo123!"
        });

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Login failed — is the database seeded?");
        }

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Token missing from login response.");
    }

    private static HttpRequestMessage CreateAuthRequest(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
