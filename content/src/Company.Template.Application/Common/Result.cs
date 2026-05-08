namespace Company.Template.Application.Common;

public sealed record Error(string Code, string Message)
{
    public static Error NotFound(string message) => new("not_found", message);

    public static Error Validation(string message) => new("validation_error", message);

    public static Error Conflict(string message) => new("conflict", message);
}

public sealed class Result<T>
{
    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(Error error) => new(default, error);
}

public sealed class Result
{
    private Result(Error? error)
    {
        Error = error;
    }

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public static Result Success() => new(null);

    public static Result Failure(Error error) => new(error);
}
