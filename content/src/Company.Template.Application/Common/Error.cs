using Company.Template.Domain.Common;

namespace Company.Template.Application.Common;

/// <summary>
///     Describes an expected application failure that can be translated by the outer boundary.
/// </summary>
/// <param name="Type">The category of the error.</param>
/// <param name="Code">A machine-readable unique identifier for the error.</param>
/// <param name="Message">A human-readable description of the error.</param>
public sealed record Error(
    ErrorType Type,
    DomainErrorCode Code,
    string Message,
    string? Target = null,
    IReadOnlyList<Error>? Details = null)
{
    public static Error None { get; } = new(ErrorType.None, DomainErrorCode.None, "No error.");

    public bool IsNone => Type == ErrorType.None;

    public static Error NotFound(string message)
    {
        return NotFound(DomainErrorCodes.NotFound, message);
    }

    public static Error NotFound(DomainErrorCode code, string message)
    {
        return Create(ErrorType.NotFound, code, message);
    }

    public static Error Validation(string message)
    {
        return Validation(DomainErrorCodes.ValidationError, message);
    }

    public static Error Validation(DomainErrorCode code,
        string message,
        string? target = null)
    {
        return Create(ErrorType.Validation, code, message, target: target);
    }

    public static Error Validation(DomainErrorCode code,
        string message,
        IReadOnlyList<Error> details)
    {
        return Create(ErrorType.Validation, code, message, details: details);
    }

    public static Error Conflict(string message)
    {
        return Conflict(DomainErrorCodes.Conflict, message);
    }

    public static Error Conflict(DomainErrorCode code, string message)
    {
        return Create(ErrorType.Conflict, code, message);
    }

    public static Error Unknown(DomainErrorCode code, string message)
    {
        return Create(ErrorType.Unknown, code, message);
    }

    internal static Error Create(ErrorType type,
        DomainErrorCode code,
        string message,
        string? target = null,
        IReadOnlyList<Error>? details = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code.Value);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new Error(type, code, message, target, details);
    }
}
