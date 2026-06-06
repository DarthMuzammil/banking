namespace Banking.Application.Abstractions;

public interface IIdempotencyRepository
{
    Task<IdempotencyCachedResponse?> GetAsync(
        Guid customerId,
        string idempotencyKey,
        string requestPath,
        CancellationToken cancellationToken = default);

    Task StoreAsync(
        Guid customerId,
        string idempotencyKey,
        string requestPath,
        int responseStatus,
        string responseBody,
        CancellationToken cancellationToken = default);
}

public sealed record IdempotencyCachedResponse(int StatusCode, string Body);
