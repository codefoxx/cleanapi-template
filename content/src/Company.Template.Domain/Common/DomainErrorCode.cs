namespace Company.Template.Domain.Common;

/// <summary>
///     Identifies a domain error with a stable machine-readable code.
/// </summary>
/// <remarks>
///     Domain error codes describe why a domain operation failed without deciding how that
///     failure should be exposed by the application or transport layer.
/// </remarks>
public sealed record DomainErrorCode
{
    private DomainErrorCode(string value)
    {
        Value = value;
    }

    /// <summary>
    ///     Represents the absence of a domain error.
    /// </summary>
    public static DomainErrorCode None { get; } = new("none");

    /// <summary>
    ///     Gets a value indicating whether this code represents the absence of an error.
    /// </summary>
    public bool IsNone => this == None;

    /// <summary>
    ///     Gets the machine-readable code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Creates a domain error code.
    /// </summary>
    public static DomainErrorCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value == None.Value
            ? None
            : new DomainErrorCode(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}

public static partial class DomainErrorCodes;
