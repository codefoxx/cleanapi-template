namespace Company.Template.Application.Common;

/// <summary>
/// Describes an expected application failure that can be translated by the outer boundary.
/// </summary>
/// <param name="Type">The category of the error.</param>
/// <param name="Code">A machine-readable unique identifier for the error.</param>
/// <param name="Message">A human-readable description of the error.</param>
public sealed record Error(ErrorType Type, string Code, string Message)
{
    public static Error None { get; } = new(ErrorType.None, "none", "No error.");

    public bool IsNone => Type == ErrorType.None;

    public static Error NotFound(string message)
    {
        return NotFound("not_found", message);
    }

    public static Error NotFound(string code, string message)
    {
        return Create(ErrorType.NotFound, code, message);
    }

    public static Error Validation(string message)
    {
        return Validation("validation_error", message);
    }

    public static Error Validation(string code, string message)
    {
        return Create(ErrorType.Validation, code, message);
    }

    public static Error Conflict(string message)
    {
        return Conflict("conflict", message);
    }

    public static Error Conflict(string code, string message)
    {
        return Create(ErrorType.Conflict, code, message);
    }

    public static Error Unknown(string code, string message)
    {
        return Create(ErrorType.Unknown, code, message);
    }

    private static Error Create(ErrorType type, string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new Error(type, code, message);
    }
}
