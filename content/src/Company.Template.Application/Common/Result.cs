namespace Company.Template.Application.Common;

/// <summary>
/// Represents the outcome of an application operation that either succeeds with a value
/// or fails with an explicit application error.
/// </summary>
/// <remarks>
/// Use cases return <see cref="Result{T}"/> instead of throwing for expected outcomes such as
/// validation failures, missing resources, or conflicts. Unexpected failures should still be exceptions.
/// Prefer <see cref="Match{TResult}"/> at boundaries where success and failure are translated,
/// for example from application results to HTTP responses.
/// </remarks>
public sealed class Result<T>
    where T : notnull
{
    private readonly T? _value;

    private Result(T? value, Error error)
    {
        _value = value;
        Error = error;
    }

    public Error Error { get; }

    public bool IsSuccess => Error.IsNone;

    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("A failure result has no value.");

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<T>(value, Error.None);
    }

    public static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error.IsNone)
        {
            throw new ArgumentException("A failure result must have an error.", nameof(error));
        }

        return new Result<T>(default, error);
    }

    public TResult Match<TResult>(
        Func<T, TResult> success,
        Func<Error, TResult> failure)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);

        return IsSuccess
            ? success(Value)
            : failure(Error);
    }
}

/// <summary>
/// Represents the outcome of an application operation that either succeeds without a value
/// or fails with an explicit application error.
/// </summary>
/// <remarks>
/// Use cases return <see cref="Result"/> instead of throwing for expected outcomes such as
/// validation failures, missing resources, or conflicts. Unexpected failures should still be exceptions.
/// Prefer <see cref="Match{TResult}"/> at boundaries where success and failure are translated,
/// for example from application results to HTTP responses.
/// </remarks>
public sealed class Result
{
    private Result(Error error)
    {
        Error = error;
    }

    public Error Error { get; }

    public bool IsSuccess => Error.IsNone;

    public bool IsFailure => !IsSuccess;

    public static Result Success()
    {
        return new Result(Error.None);
    }

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error.IsNone)
        {
            throw new ArgumentException("A failure result must have an error.", nameof(error));
        }

        return new Result(error);
    }

    public TResult Match<TResult>(
        Func<TResult> success,
        Func<Error, TResult> failure)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);

        return IsSuccess
            ? success()
            : failure(Error);
    }
}
