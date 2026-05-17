using Company.Template.Domain.Common;

namespace Company.Template.Application.Common;

/// <summary>
///     Identifies an application error with a stable machine-readable code.
/// </summary>
/// <remarks>
///     Application error codes belong to the application boundary and may be exposed through APIs,
///     logs, telemetry, or other outer adapters. Domain error codes can be translated into this type
///     by preserving their value without exposing the domain-owned type from application errors.
/// </remarks>
public sealed record ErrorCode
{
    private ErrorCode(string value)
    {
        Value = value;
    }

    public static ErrorCode None { get; } = new("none");

    public bool IsNone => this == None;

    public string Value { get; }

    public static ErrorCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value == None.Value
            ? None
            : new ErrorCode(value);
    }

    public static ErrorCode FromDomain(DomainErrorCode code)
    {
        ArgumentNullException.ThrowIfNull(code);

        return Create(code.Value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}

public static class ErrorCodes
{
    public static readonly ErrorCode Conflict =
        ErrorCode.Create("conflict");

    public static readonly ErrorCode NotFound =
        ErrorCode.Create("not_found");

    public static readonly ErrorCode ValidationError =
        ErrorCode.Create("validation_error");
}