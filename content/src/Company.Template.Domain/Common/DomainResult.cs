namespace Company.Template.Domain.Common;

/// <summary>
///     Represents the outcome of a domain operation that either succeeds with a value
///     or fails with an explicit domain error.
/// </summary>
/// <remarks>
///     Domain operations return <see cref="DomainResult{T}" /> when an expected domain
///     validation or business-rule failure should be communicated without throwing.
///     Unexpected failures and programming errors may still use exceptions.
/// </remarks>
/// <typeparam name="T">The type of the successful value.</typeparam>
public sealed class DomainResult<T>
    where T : notnull
{
    private readonly T? _value;

    private DomainResult(T? value, DomainError error)
    {
        _value = value;
        Error = error;
    }

    /// <summary>
    ///     Gets the domain error for failed results, or <see cref="DomainError.None" /> for successful results.
    /// </summary>
    public DomainError Error { get; }

    /// <summary>
    ///     Gets a value indicating whether the result failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    ///     Gets a value indicating whether the result succeeded.
    /// </summary>
    public bool IsSuccess => Error.IsNone;

    /// <summary>
    ///     Gets the successful value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the result is a failure.
    /// </exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("A failure result has no value.");

    /// <summary>
    ///     Creates a successful result.
    /// </summary>
    public static DomainResult<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new DomainResult<T>(value, DomainError.None);
    }

    /// <summary>
    ///     Creates a failed result.
    /// </summary>
    public static DomainResult<T> Failure(DomainError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error.IsNone)
        {
            throw new ArgumentException("A failure result must have an error.", nameof(error));
        }

        return new DomainResult<T>(default, error);
    }

    /// <summary>
    ///     Handles both success and failure cases.
    /// </summary>
    public TResult Match<TResult>(
        Func<T, TResult> success,
        Func<DomainError, TResult> failure)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);

        return IsSuccess
            ? success(Value)
            : failure(Error);
    }
}

/// <summary>
///     Represents the outcome of a domain operation that either succeeds without a value
///     or fails with an explicit domain error.
/// </summary>
/// <remarks>
///     Use this type for aggregate business operations that may fail for expected domain
///     reasons, such as lifecycle conflicts or violated business rules.
/// </remarks>
public sealed class DomainResult
{
    private DomainResult(DomainError error)
    {
        Error = error;
    }

    /// <summary>
    ///     Gets the domain error for failed results, or <see cref="DomainError.None" /> for successful results.
    /// </summary>
    public DomainError Error { get; }

    /// <summary>
    ///     Gets a value indicating whether the result failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    ///     Gets a value indicating whether the result succeeded.
    /// </summary>
    public bool IsSuccess => Error.IsNone;

    /// <summary>
    ///     Creates a successful result.
    /// </summary>
    public static DomainResult Success()
    {
        return new DomainResult(DomainError.None);
    }

    /// <summary>
    ///     Creates a failed result.
    /// </summary>
    public static DomainResult Failure(DomainError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error.IsNone)
        {
            throw new ArgumentException("A failure result must have an error.", nameof(error));
        }

        return new DomainResult(error);
    }

    /// <summary>
    ///     Handles both success and failure cases.
    /// </summary>
    public TResult Match<TResult>(
        Func<TResult> success,
        Func<DomainError, TResult> failure)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);

        return IsSuccess
            ? success()
            : failure(Error);
    }
}
