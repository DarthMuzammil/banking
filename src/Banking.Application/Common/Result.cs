namespace Banking.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);

    public static Result Failure(string error, string errorCode) =>
        new(false, error, errorCode);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(true, null, null) => Value = value;

    private Result(string error, string errorCode) : base(false, error, errorCode) => Value = default;

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(string error, string errorCode) => new(error, errorCode);
}
