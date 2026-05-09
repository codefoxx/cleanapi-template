namespace Company.Template.Application.Common;

public sealed record Error(string Code, string Message)
{
    public static Error NotFound(string message)
    {
        return new Error("not_found", message);
    }

    public static Error Validation(string message)
    {
        return new Error("validation_error", message);
    }

    public static Error Conflict(string message)
    {
        return new Error("conflict", message);
    }
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

    public static Result<T> Success(T value)
    {
        return new Result<T>(value, null);
    }

    public static Result<T> Failure(Error error)
    {
        return new Result<T>(default, error);
    }
}

public sealed class Result
{
    private Result(Error? error)
    {
        Error = error;
    }

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public static Result Success()
    {
        return new Result(null);
    }

    public static Result Failure(Error error)
    {
        return new Result(error);
    }
}
