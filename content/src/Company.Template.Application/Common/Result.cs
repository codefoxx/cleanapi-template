namespace Company.Template.Application.Common;

/// <summary>
/// Specifies the type of error that occurred during an operation.
/// </summary>
public enum ErrorType
{
    /// <summary>The operation failed because of invalid input.</summary>
    Validation,
    /// <summary>The requested resource was not found.</summary>
    NotFound,
    /// <summary>The operation conflicted with the current state of the system.</summary>
    Conflict
}

/// <summary>
/// Represents an error that occurred during an operation.
/// </summary>
/// <param name="Type">The category of the error.</param>
/// <param name="Code">A machine-readable unique identifier for the error.</param>
/// <param name="Message">A human-readable description of the error.</param>
public sealed record Error(ErrorType Type, string Code, string Message)
{
    public static Error NotFound(string message)
    {
        return new Error(ErrorType.NotFound, "not_found", message);
    }

    public static Error Validation(string message)
    {
        return new Error(ErrorType.Validation, "validation_error", message);
    }

    public static Error Conflict(string message)
    {
        return new Error(ErrorType.Conflict, "conflict", message);
    }
}

/// <summary>
/// Represents the outcome of an operation that may return a value or an error.
/// </summary>
/// <remarks>
/// This type encourages functional error handling by making the possibility of failure
/// explicit in the API. It should be used for expected business failures instead of exceptions.
/// </remarks>
/// <typeparam name="T">The type of the result value.</typeparam>
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

/// <summary>
/// Represents the outcome of an operation that may return an error but no value.
/// </summary>
/// <remarks>
/// Use this type for commands or operations where the success state is enough information,
/// while still requiring explicit error handling for failures.
/// </remarks>
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
