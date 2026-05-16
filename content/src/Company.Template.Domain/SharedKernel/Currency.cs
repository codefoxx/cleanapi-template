using System.Diagnostics.CodeAnalysis;
using Company.Template.Domain.Common;

namespace Company.Template.Domain.SharedKernel;

/// <summary>
///     A value object that normalizes currency codes and supports safe
///     <see cref="Money" /> equality and operations.
/// </summary>
/// <remarks>
///     Currency codes are normalized to uppercase ISO 4217 alphabetic codes.
///     This type validates the expected ISO 4217 code shape and accepts the
///     currency codes supported by this application.
///     Use <see cref="TryCreate(string?, out Currency?, out DomainError?)" /> when validating raw input
///     that may fail as part of a normal application flow.
/// </remarks>
public sealed record Currency
{
    /// <summary>
    ///     The expected length of a normalized currency code.
    /// </summary>
    public const int CodeLength = 3;

    private Currency(string code, string symbol)
    {
        Code = code;
        Symbol = symbol;
    }

    /// <summary>
    ///     Represents the absence of a currency.
    /// </summary>
    /// <remarks>
    ///     This is used for neutral money values such as <see cref="Money.Zero()" />.
    ///     Non-zero money values require a real currency.
    /// </remarks>
    public static Currency Empty { get; } = new(string.Empty, string.Empty);

    /// <summary>
    ///     Gets the normalized currency code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    ///     Gets a value indicating whether this instance represents no currency.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Code);

    /// <summary>
    ///     Gets the display symbol used for this currency.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    ///     Creates a currency from a code that is expected to be valid.
    /// </summary>
    /// <param name="code">The currency code.</param>
    /// <returns>A valid <see cref="Currency" /> instance.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="code" /> is missing, whitespace, not a valid
    ///     three-letter ISO 4217 alphabetic code, or not supported by this application.
    /// </exception>
    /// <remarks>
    ///     The currency symbol is set to the normalized code. For expected validation failures from
    ///     raw input, prefer <see cref="TryCreate(string?, out Currency?, out DomainError?)" />.
    /// </remarks>
    public static Currency Create(string code)
    {
        return TryCreate(code, out Currency? currency, out DomainError? error)
            ? currency
            : throw new ArgumentException(error.Message, nameof(code));
    }

    /// <summary>
    ///     Creates a currency from a code and display symbol that are expected to be valid.
    /// </summary>
    /// <param name="code">The currency code.</param>
    /// <param name="symbol">The display symbol.</param>
    /// <returns>A valid <see cref="Currency" /> instance.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="code" /> or <paramref name="symbol" /> is missing or whitespace,
    ///     when <paramref name="code" /> is not a valid three-letter ISO 4217 alphabetic code,
    ///     or when the code is not supported by this application.
    /// </exception>
    /// <remarks>
    ///     For expected validation failures from raw input, prefer
    ///     <see cref="TryCreate(string?, string?, out Currency?, out DomainError?)" />.
    /// </remarks>
    public static Currency Create(string code, string symbol)
    {
        return TryCreate(code, symbol, out Currency? currency, out DomainError? error)
            ? currency
            : throw new ArgumentException(error.Message, nameof(code));
    }

    /// <summary>
    ///     Attempts to create a valid currency without throwing for expected validation failures.
    /// </summary>
    public static bool TryCreate(
        string? code,
        [NotNullWhen(true)] out Currency? currency,
        [NotNullWhen(false)] out DomainError? error)
    {
        DomainResult<Currency> result = CreateCurrency(code);

        currency = result.IsSuccess ? result.Value : null;
        error = result.IsSuccess ? null : result.Error;

        return result.IsSuccess;
    }

    /// <summary>
    ///     Attempts to create a valid currency with a display symbol without throwing for expected validation failures.
    /// </summary>
    public static bool TryCreate(
        string? code,
        string? symbol,
        [NotNullWhen(true)] out Currency? currency,
        [NotNullWhen(false)] out DomainError? error)
    {
        DomainResult<Currency> result = CreateCurrency(code, symbol);

        currency = result.IsSuccess ? result.Value : null;
        error = result.IsSuccess ? null : result.Error;

        return result.IsSuccess;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Code;
    }

    private static DomainResult<Currency> CreateCurrency(string? code)
    {
        return ValidateCurrencyCode(code)
           .Map(validCode => new Currency(validCode, validCode));
    }

    private static DomainResult<Currency> CreateCurrency(string? code, string? symbol)
    {
        return ValidateCurrencyCode(code)
           .Bind(validCode => ValidateSymbol(symbol)
               .Map(validSymbol => new Currency(validCode, validSymbol)));
    }

    private static DomainResult<string> ValidateCurrencyCode(string? code)
    {
        return RequireCode(code)
              .Map(Normalize)
              .Bind(EnsureIso4217Format)
              .Bind(EnsureSupportedCurrencyCode);
    }

    private static DomainResult<string> RequireCode(string? code)
    {
        return string.IsNullOrWhiteSpace(code)
            ? DomainResult<string>.Failure(DomainError.Create(
                DomainErrorCodes.CurrencyRequired,
                "Currency is required."))
            : DomainResult<string>.Success(code);
    }

    private static DomainResult<string> ValidateSymbol(string? symbol)
    {
        return string.IsNullOrWhiteSpace(symbol)
            ? DomainResult<string>.Failure(DomainError.Create(
                DomainErrorCodes.CurrencySymbolRequired,
                "Currency symbol is required."))
            : DomainResult<string>.Success(symbol.Trim());
    }

    private static string Normalize(string code)
    {
        return code.Trim().ToUpperInvariant();
    }

    private static DomainResult<string> EnsureIso4217Format(string code)
    {
        return HasValidFormat(code)
            ? DomainResult<string>.Success(code)
            : DomainResult<string>.Failure(DomainError.Create(
                DomainErrorCodes.CurrencyInvalidFormat,
                "Currency must be a three-letter ISO 4217 alphabetic code."));
    }

    private static DomainResult<string> EnsureSupportedCurrencyCode(string code)
    {
        return Iso4217CurrencyCodes.Contains(code)
            ? DomainResult<string>.Success(code)
            : DomainResult<string>.Failure(DomainError.Create(
                DomainErrorCodes.CurrencyUnsupported,
                "Currency is not supported by this application."));
    }

    private static bool HasValidFormat(string code)
    {
        return code.Length == CodeLength
         && code.All(static character => character is >= 'A' and <= 'Z');
    }
}
