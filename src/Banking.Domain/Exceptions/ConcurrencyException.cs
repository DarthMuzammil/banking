namespace Banking.Domain.Exceptions;

public sealed class ConcurrencyException : Exception
{
    public string ResourceType { get; }
    public Guid ResourceId { get; }

    public ConcurrencyException(string resourceType, Guid resourceId)
        : base($"A concurrent update conflict occurred for {resourceType} {resourceId}.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}
