namespace Company.Template.Application.Common;

/// <summary>
///     Describes an expected application failure that can be translated by the outer boundary.
/// </summary>
/// <param name="Type">The category of the error.</param>
/// <param name="Code">A machine-readable unique identifier for the error.</param>
/// <param name="Message">A human-readable description of the error.</param>
public sealed record Error(
    ErrorType Type,
    ErrorCode Code,
    string Message,
    string? Target = null,
    IReadOnlyList<Error>? Details = null)
{
    public static Error None { get; } = new(ErrorType.None, ErrorCode.None, "No error.");

    public bool IsNone => Type == ErrorType.None;

    public static Error NotFound(string message)
    {
        return NotFound(ErrorCodes.NotFound, message);
    }

    public static Error NotFound(ErrorCode code, string message)
    {
        return Create(ErrorType.NotFound, code, message);
    }

    public static Error Validation(string message)
    {
        return Validation(ErrorCodes.ValidationError, message);
    }

    public static Error Validation(
        ErrorCode code,
        string message,
        string? target = null)
    {
        return Create(ErrorType.Validation, code, message, target: target);
    }

    public static Error Validation(
        ErrorCode code,
        string message,
        IReadOnlyList<Error> details)
    {
        return Create(ErrorType.Validation, code, message, details: details);
    }

    public static Error Conflict(string message)
    {
        return Conflict(ErrorCodes.Conflict, message);
    }

    public static Error Conflict(ErrorCode code, string message)
    {
        return Create(ErrorType.Conflict, code, message);
    }

    public static Error Unknown(ErrorCode code, string message)
    {
        return Create(ErrorType.Unknown, code, message);
    }

    internal static Error Create(
        ErrorType type,
        ErrorCode code,
        string message,
        string? target = null,
        IReadOnlyList<Error>? details = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (type == ErrorType.None && !code.IsNone)
        {
            throw new ArgumentException("A none error must use the none code.", nameof(code));
        }

        if (type != ErrorType.None && code.IsNone)
        {
            throw new ArgumentException("An application error must have an error code.", nameof(code));
        }

        return new Error(type, code, message, target, details);
    }
}
