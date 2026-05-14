using System.Diagnostics.CodeAnalysis;
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

/// <summary>
///     Represents a monetary value with a specific currency.
/// </summary>
/// <remarks>
///     <see cref="Money" /> is a value object that ensures mathematical consistency and enforces rules
///     such as matching currencies for operations and limiting amounts to a standard scale.
///     Use <c>TryCreate</c> when validating raw input that may fail as part of a normal application flow.
///     Use <c>Create</c> when the caller is expected to provide already valid values.
/// </remarks>
public sealed record Money : IComparable<Money>
{
    /// <summary>
    ///     The standard number of decimal places for monetary values.
    /// </summary>
    public const int Scale = 2;

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    ///     Gets the monetary amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    ///     Gets the currency of the monetary amount.
    /// </summary>
    public Currency Currency { get; }

    /// <summary>
    ///     Gets a value indicating whether this instance represents the neutral zero money value.
    /// </summary>
    public bool IsZero => Amount == 0 && Currency == Currency.Empty;

    /// <inheritdoc />
    public int CompareTo(Money? other)
    {
        if (other is null)
        {
            return 1;
        }

        if (IsZero || other.IsZero)
        {
            return Amount.CompareTo(other.Amount);
        }

        EnsureSameCurrency(other);

        return Amount.CompareTo(other.Amount);
    }

    /// <summary>
    ///     Creates a money value from values that are expected to be valid.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency.</param>
    /// <returns>A valid <see cref="Money" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="currency" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="amount" /> is negative.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when a non-zero amount has no currency, or when the amount has more than
    ///     <see cref="Scale" /> decimal places.
    /// </exception>
    /// <remarks>
    ///     This method is intentionally strict. For expected validation failures from raw input,
    ///     prefer <c>TryCreate</c> so the caller can translate the returned <see cref="DomainError" /> explicitly.
    /// </remarks>
    public static Money Create(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        return amount switch
        {
            < 0 => throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Price cannot be negative."),

            > 0 when currency.IsEmpty => throw new ArgumentException(
                "Currency is required when amount is greater than zero.",
                nameof(currency)),

            _ when HasMoreDecimalPlacesThan(amount, Scale) => throw new ArgumentException(
                $"Price cannot have more than {Scale} decimal places.",
                nameof(amount)),

            _ => new Money(amount, currency)
        };
    }

    /// <summary>
    ///     Creates a money value from an amount and currency code that are expected to be valid.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency code.</param>
    /// <returns>A valid <see cref="Money" /> instance.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when the currency code or amount does not satisfy the money invariants.
    /// </exception>
    /// <remarks>
    ///     This method delegates currency creation to <see cref="Currency.Create(string)" />.
    ///     For expected validation failures from raw input, prefer <see cref="TryCreate(decimal, string, out Money?, out DomainError?)" />.
    /// </remarks>
    public static Money Create(decimal amount, string currency)
    {
        return Create(amount, Currency.Create(currency));
    }

    /// <summary>
    ///     Attempts to create a valid money value without throwing for expected validation failures.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="money">
    ///     The created money value when the method returns <see langword="true" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <param name="error">
    ///     The domain error when the method returns <see langword="false" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when a valid money value could be created;
    ///     otherwise <see langword="false" />.
    /// </returns>
    public static bool TryCreate(
        decimal amount,
        Currency? currency,
        [NotNullWhen(true)] out Money? money,
        [NotNullWhen(false)] out DomainError? error)
    {
        money = null;

        if (currency is null)
        {
            error = DomainError.Create(
                DomainErrorCodes.CurrencyRequired,
                "Currency is required.");
            return false;
        }

        if (amount < 0)
        {
            error = DomainError.Create(
                DomainErrorCodes.AmountNegative,
                "Amount cannot be negative.");
            return false;
        }

        if (amount > 0 && currency.IsEmpty)
        {
            error = DomainError.Create(
                DomainErrorCodes.CurrencyRequired,
                "Currency is required when amount is greater than zero.");
            return false;
        }

        if (HasMoreDecimalPlacesThan(amount, Scale))
        {
            error = DomainError.Create(
                DomainErrorCodes.AmountTooManyDecimalPlaces,
                $"Price cannot have more than {Scale} decimal places.");
            return false;
        }

        money = new Money(amount, currency);
        error = null;

        return true;
    }

    /// <summary>
    ///     Attempts to create a valid money value from an amount and currency code without throwing
    ///     for expected validation failures.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The raw currency code input.</param>
    /// <param name="money">
    ///     The created money value when the method returns <see langword="true" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <param name="error">
    ///     The domain error when the method returns <see langword="false" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when a valid money value could be created;
    ///     otherwise <see langword="false" />.
    /// </returns>
    public static bool TryCreate(
        decimal amount,
        string currency,
        [NotNullWhen(true)] out Money? money,
        [NotNullWhen(false)] out DomainError? error)
    {
        if (!Currency.TryCreate(currency, out Currency? parsedCurrency, out error))
        {
            money = null;
            return false;
        }

        return TryCreate(amount, parsedCurrency, out money, out error);
    }

    /// <summary>
    ///     Creates a money value by rounding the amount to <see cref="Scale" /> decimal places.
    /// </summary>
    /// <param name="amount">The amount to round.</param>
    /// <param name="currency">The currency.</param>
    /// <returns>A valid rounded <see cref="Money" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="currency" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the rounded value does not satisfy the money invariants.
    /// </exception>
    public static Money CreateRounded(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        decimal roundedAmount = decimal.Round(
            amount,
            Scale,
            MidpointRounding.AwayFromZero);

        return Create(roundedAmount, currency);
    }

    /// <summary>
    ///     Creates the neutral zero money value without a currency.
    /// </summary>
    /// <returns>A zero money value using <see cref="Currency.Empty" />.</returns>
    public static Money Zero()
    {
        return Zero(Currency.Empty);
    }

    /// <summary>
    ///     Creates a zero money value for the specified currency.
    /// </summary>
    /// <param name="currency">The currency of the zero value.</param>
    /// <returns>A zero money value.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="currency" /> is <see langword="null" />.
    /// </exception>
    public static Money Zero(Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        return new Money(0, currency);
    }

    /// <summary>
    ///     Adds another money value with the same currency.
    /// </summary>
    /// <param name="other">The money value to add.</param>
    /// <returns>The sum of both money values.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="other" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the currencies do not match.
    /// </exception>
    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        EnsureSameCurrency(other);

        return Create(Amount + other.Amount, Currency);
    }

    /// <summary>
    ///     Subtracts another money value with the same currency.
    /// </summary>
    /// <param name="other">The money value to subtract.</param>
    /// <returns>The difference between both money values.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="other" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the currencies do not match or the result would be negative.
    /// </exception>
    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        EnsureSameCurrency(other);

        if (Amount < other.Amount)
        {
            throw new InvalidOperationException("Cannot subtract more than the current amount.");
        }

        return Create(Amount - other.Amount, Currency);
    }

    /// <summary>
    ///     Scales the money value by a non-negative factor and rounds the result.
    /// </summary>
    /// <param name="factor">The factor to multiply the amount by.</param>
    /// <returns>The scaled and rounded money value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="factor" /> is negative.
    /// </exception>
    public Money ScaleBy(decimal factor)
    {
        if (factor < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(factor),
                "Factor cannot be negative.");
        }

        return CreateRounded(Amount * factor, Currency);
    }

    /// <summary>
    ///     Adds two money values.
    /// </summary>
    public static Money operator +(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.Add(right);
    }

    /// <summary>
    ///     Subtracts the right money value from the left money value.
    /// </summary>
    public static Money operator -(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.Subtract(right);
    }

    /// <summary>
    ///     Determines whether the left money value is less than the right money value.
    /// </summary>
    public static bool operator <(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     Determines whether the left money value is less than or equal to the right money value.
    /// </summary>
    public static bool operator <=(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     Determines whether the left money value is greater than the right money value.
    /// </summary>
    public static bool operator >(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     Determines whether the left money value is greater than or equal to the right money value.
    /// </summary>
    public static bool operator >=(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.CompareTo(right) >= 0;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Currency.IsEmpty
            ? Amount.ToString("0.00", CultureInfo.InvariantCulture)
            : $"{Amount.ToString("0.00", CultureInfo.InvariantCulture)} {Currency.Code}";
    }

    private static bool HasMoreDecimalPlacesThan(decimal value, int scale)
    {
        return decimal.Round(value, scale) != value;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency == other.Currency)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Cannot operate on money values with different currencies: '{Currency.Code}' and '{other.Currency.Code}'.");
    }
}
