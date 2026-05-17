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

public static partial class DomainErrorCodes
{
    public static readonly DomainErrorCode AmountNegative =
        DomainErrorCode.Create("amount_negative");

    public static readonly DomainErrorCode AmountTooManyDecimalPlaces =
        DomainErrorCode.Create("amount_too_many_decimal_places");

    public static readonly DomainErrorCode Conflict =
        DomainErrorCode.Create("conflict");

    public static readonly DomainErrorCode CurrencyInvalidFormat =
        DomainErrorCode.Create("currency_invalid_format");

    public static readonly DomainErrorCode CurrencyRequired =
        DomainErrorCode.Create("currency_required");

    public static readonly DomainErrorCode CurrencySymbolRequired =
        DomainErrorCode.Create("currency_symbol_required");

    public static readonly DomainErrorCode CurrencyUnsupported =
        DomainErrorCode.Create("currency_unsupported");

    public static readonly DomainErrorCode NotFound =
        DomainErrorCode.Create("not_found");

    public static readonly DomainErrorCode ValidationError =
        DomainErrorCode.Create("validation_error");
}
