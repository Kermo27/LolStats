namespace LolStatsTracker.Shared.DTOs;

public class Result<T>
{
    public bool IsSuccess { get; protected init; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; protected init; }
    public string? Error { get; protected init; }
    public string? ErrorCode { get; protected init; }

    protected Result() { }

    public static Result<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    public static Result<T> Failure(string error, string? errorCode = null) => new()
    {
        IsSuccess = false,
        Error = error,
        ErrorCode = errorCode ?? ErrorCodes.InternalError
    };

    public static Result<T> NotFound(string message = "Resource not found") => new()
    {
        IsSuccess = false,
        Error = message,
        ErrorCode = ErrorCodes.NotFound
    };

    public static Result<T> Unauthorized(string message = "Unauthorized") => new()
    {
        IsSuccess = false,
        Error = message,
        ErrorCode = ErrorCodes.Unauthorized
    };

    public static Result<T> ValidationError(string message) => new()
    {
        IsSuccess = false,
        Error = message,
        ErrorCode = ErrorCodes.ValidationError
    };

    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (IsFailure)
            return Result<TNew>.Failure(Error!, ErrorCode);
        
        return Result<TNew>.Success(mapper(Value!));
    }
}

public class Result
{
    public bool IsSuccess { get; protected init; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; protected init; }
    public string? ErrorCode { get; protected init; }

    protected Result() { }

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(string error, string? errorCode = null) => new()
    {
        IsSuccess = false,
        Error = error,
        ErrorCode = errorCode ?? ErrorCodes.InternalError
    };

    public static Result NotFound(string message = "Resource not found") => new()
    {
        IsSuccess = false,
        Error = message,
        ErrorCode = ErrorCodes.NotFound
    };

    public static Result Unauthorized(string message = "Unauthorized") => new()
    {
        IsSuccess = false,
        Error = message,
        ErrorCode = ErrorCodes.Unauthorized
    };

    public static Result ValidationError(string message) => new()
    {
        IsSuccess = false,
        Error = message,
        ErrorCode = ErrorCodes.ValidationError
    };
}
