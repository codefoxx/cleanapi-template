using System.Diagnostics.CodeAnalysis;
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

/// <summary>
///     A value object that normalizes currency codes and supports safe
///     <see cref="Money" /> equality and operations.
/// </summary>
/// <remarks>
///     Currency codes are normalized to uppercase three-letter codes. This type checks the expected
///     code shape but does not validate the value against a complete ISO 4217 currency list.
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
    ///     Thrown when <paramref name="code" /> is missing, whitespace, or not exactly
    ///     <see cref="CodeLength" /> characters after trimming.
    /// </exception>
    /// <remarks>
    ///     The currency symbol is set to the normalized code. For expected validation failures from
    ///     raw input, prefer <see cref="TryCreate(string?, out Currency?, out DomainError?)" />.
    /// </remarks>
    public static Currency Create(string code)
    {
        Guard.ThrowIfNullOrWhiteSpace(code, nameof(code), "Currency is required.");

        string normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length != CodeLength)
        {
            throw new ArgumentException("Currency must be an ISO 4217 three-letter code.", nameof(code));
        }

        return new Currency(normalizedCode, normalizedCode);
    }

    /// <summary>
    ///     Creates a currency from a code and display symbol that are expected to be valid.
    /// </summary>
    /// <param name="code">The currency code.</param>
    /// <param name="symbol">The display symbol.</param>
    /// <returns>A valid <see cref="Currency" /> instance.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="code" /> or <paramref name="symbol" /> is missing or whitespace,
    ///     or when <paramref name="code" /> is not exactly <see cref="CodeLength" /> characters after trimming.
    /// </exception>
    /// <remarks>
    ///     For expected validation failures from raw input, prefer
    ///     <see cref="TryCreate(string?, string?, out Currency?, out DomainError?)" />.
    /// </remarks>
    public static Currency Create(string code, string symbol)
    {
        Guard.ThrowIfNullOrWhiteSpace(code, nameof(code), "Currency is required.");
        Guard.ThrowIfNullOrWhiteSpace(symbol, nameof(symbol), "Currency symbol is required.");

        string normalizedCode = code.Trim().ToUpperInvariant();
        string normalizedSymbol = symbol.Trim();

        if (normalizedCode.Length != CodeLength)
        {
            throw new ArgumentException("Currency must be an ISO 4217 three-letter code.", nameof(code));
        }

        return new Currency(normalizedCode, normalizedSymbol);
    }

    /// <summary>
    ///     Attempts to create a valid currency without throwing for expected validation failures.
    /// </summary>
    /// <param name="code">The raw currency code input.</param>
    /// <param name="currency">
    ///     The created currency when the method returns <see langword="true" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <param name="error">
    ///     The domain error when the method returns <see langword="false" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when a valid currency could be created;
    ///     otherwise <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     The currency symbol is set to the normalized code.
    /// </remarks>
    public static bool TryCreate(
        string? code,
        [NotNullWhen(true)] out Currency? currency,
        [NotNullWhen(false)] out DomainError? error)
    {
        currency = null;

        if (string.IsNullOrWhiteSpace(code))
        {
            error = DomainError.Create(
                DomainErrorCodes.CurrencyRequired,
                "Currency is required.");
            return false;
        }

        string normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length != CodeLength)
        {
            error = DomainError.Create(
                DomainErrorCodes.CurrencyInvalidFormat,
                "Currency must be an ISO 4217 three-letter code.");
            return false;
        }

        currency = new Currency(normalizedCode, normalizedCode);
        error = null;

        return true;
    }

    /// <summary>
    ///     Attempts to create a valid currency with a display symbol without throwing for expected validation failures.
    /// </summary>
    /// <param name="code">The raw currency code input.</param>
    /// <param name="symbol">The raw display symbol input.</param>
    /// <param name="currency">
    ///     The created currency when the method returns <see langword="true" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <param name="error">
    ///     The domain error when the method returns <see langword="false" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when a valid currency could be created;
    ///     otherwise <see langword="false" />.
    /// </returns>
    public static bool TryCreate(
        string? code,
        string? symbol,
        [NotNullWhen(true)] out Currency? currency,
        [NotNullWhen(false)] out DomainError? error)
    {
        currency = null;

        if (string.IsNullOrWhiteSpace(code))
        {
            error = DomainError.Create(
                DomainErrorCodes.CurrencyRequired,
                "Currency is required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(symbol))
        {
            error = DomainError.Create(
                DomainErrorCodes.CurrencySymbolRequired,
                "Currency symbol is required.");
            return false;
        }

        string normalizedCode = code.Trim().ToUpperInvariant();
        string normalizedSymbol = symbol.Trim();

        if (normalizedCode.Length != CodeLength)
        {
            error = DomainError.Create(
                DomainErrorCodes.CurrencyInvalidFormat,
                "Currency must be an ISO 4217 three-letter code.");
            return false;
        }

        currency = new Currency(normalizedCode, normalizedSymbol);
        error = null;

        return true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Code;
    }
}
