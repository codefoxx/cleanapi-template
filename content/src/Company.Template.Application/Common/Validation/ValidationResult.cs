namespace Company.Template.Application.Common.Validation;

public sealed class ValidationResult<T>
    where T : notnull
{
    private readonly T? _value;

    private ValidationResult(T value)
    {
        _value = value;
        Errors = [];
    }

    private ValidationResult(IReadOnlyList<Error> errors)
    {
        _value = default;
        Errors = errors;
    }

    public IReadOnlyList<Error> Errors { get; }

    public bool IsValid => Errors.Count == 0;

    public T Value => IsValid
        ? _value!
        : throw new InvalidOperationException("Validation result has no value.");

    public static ValidationResult<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new ValidationResult<T>(value);
    }

    public static ValidationResult<T> Failure(IReadOnlyList<Error> errors)
    {
        if (errors.Count == 0)
        {
            throw new ArgumentException("Validation failure requires at least one error.", nameof(errors));
        }

        return new ValidationResult<T>(errors);
    }

    public Result<T> ToResult()
    {
        return IsValid
            ? Result<T>.Success(Value)
            : Result<T>.Failure(Error.Validation(
                ErrorCodes.ValidationError,
                "One or more validation errors occurred.",
                Errors));
    }
}
