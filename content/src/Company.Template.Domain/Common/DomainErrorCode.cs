namespace Company.Template.Domain.Common;

/// <summary>
///     A stable, machine-readable code that identifies a domain validation or business-rule failure.
/// </summary>
public sealed record DomainErrorCode
{
    private DomainErrorCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DomainErrorCode Create(string value)
    {
        Guard.ThrowIfNullOrWhiteSpace(
            value,
            nameof(value),
            "Domain error code is required.");

        return new DomainErrorCode(value.Trim());
    }

    public override string ToString()
    {
        return Value;
    }
}

/// <summary>
///     Describes a domain-level failure without deciding how the application should expose it.
/// </summary>
public sealed record DomainError(DomainErrorCode Code, string Message);

public static partial class DomainErrorCodes;
