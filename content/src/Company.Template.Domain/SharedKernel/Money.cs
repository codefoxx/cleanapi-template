using System.Diagnostics.CodeAnalysis;
using Company.Template.Domain.Common;

namespace Company.Template.Domain.SharedKernel;

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
    public const int Scale = 2;

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public Currency Currency { get; }

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

    /// <remarks>
    ///     This method is intentionally strict. For expected validation failures from raw input,
    ///     prefer <c>TryCreate</c> so the caller can translate the returned <see cref="DomainError" /> explicitly.
    /// </remarks>
    public static Money Create(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        if (TryCreate(amount, currency, out Money? money, out DomainError? error))
        {
            return money;
        }

        throw error.Code == DomainErrorCodes.AmountNegative
            ? new ArgumentOutOfRangeException(nameof(amount), error.Message)
            : new ArgumentException(error.Message, nameof(amount));
    }

    /// <remarks>
    ///     This method delegates currency creation to <see cref="Currency.Create(string)" />.
    ///     For expected validation failures from raw input, prefer
    ///     <see cref="TryCreate(decimal, string, out Money?, out DomainError?)" />.
    /// </remarks>
    public static Money Create(decimal amount, string currency)
    {
        if (TryCreate(amount, currency, out Money? money, out DomainError? error))
        {
            return money;
        }

        throw error.Code == DomainErrorCodes.AmountNegative
            ? new ArgumentOutOfRangeException(nameof(amount), error.Message)
            : new ArgumentException(error.Message, nameof(currency));
    }

    public static bool TryCreate(
        decimal amount,
        Currency? currency,
        [NotNullWhen(true)] out Money? money,
        [NotNullWhen(false)] out DomainError? error)
    {
        DomainResult<Money> result = CreateMoney(amount, currency);

        money = result.IsSuccess ? result.Value : null;
        error = result.IsSuccess ? null : result.Error;

        return result.IsSuccess;
    }

    public static bool TryCreate(
        decimal amount,
        string currency,
        [NotNullWhen(true)] out Money? money,
        [NotNullWhen(false)] out DomainError? error)
    {
        DomainResult<Money> result = CreateMoney(amount, currency);

        money = result.IsSuccess ? result.Value : null;
        error = result.IsSuccess ? null : result.Error;

        return result.IsSuccess;
    }

    public static Money CreateRounded(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        decimal roundedAmount = decimal.Round(
            amount,
            Scale,
            MidpointRounding.AwayFromZero);

        return Create(roundedAmount, currency);
    }

    /// <remarks>
    ///     The neutral zero value has no currency. Use <see cref="Zero(Currency)" /> when the zero value should
    ///     participate in normal money operations for a specific currency.
    /// </remarks>
    public static Money Zero()
    {
        return Zero(Currency.Empty);
    }

    public static Money Zero(Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        return new Money(0, currency);
    }

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        EnsureSameCurrency(other);

        return Create(Amount + other.Amount, Currency);
    }

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

    public static Money operator +(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.Add(right);
    }

    public static Money operator -(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.Subtract(right);
    }

    public static bool operator <(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.CompareTo(right) > 0;
    }

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

    private static DomainResult<Money> CreateMoney(decimal amount, string currency)
    {
        return CreateCurrency(currency)
           .Bind(validCurrency => CreateMoney(amount, validCurrency));
    }

    private static DomainResult<Money> CreateMoney(decimal amount, Currency? currency)
    {
        return RequireCurrency(currency)
              .Bind(validCurrency => EnsureAmountIsNotNegative(amount, validCurrency))
              .Bind(validCurrency => EnsureCurrencyIsPresentWhenAmountIsPositive(amount, validCurrency))
              .Bind(validCurrency => EnsureAmountScaleIsValid(amount, validCurrency))
              .Map(validCurrency => new Money(amount, validCurrency));
    }

    private static DomainResult<Currency> CreateCurrency(string currency)
    {
        return Currency.TryCreate(currency, out Currency? parsedCurrency, out DomainError? error)
            ? DomainResult<Currency>.Success(parsedCurrency)
            : DomainResult<Currency>.Failure(error);
    }

    private static DomainResult<Currency> RequireCurrency(Currency? currency)
    {
        return currency is null
            ? DomainResult<Currency>.Failure(DomainError.Create(
                DomainErrorCodes.CurrencyRequired,
                "Currency is required."))
            : DomainResult<Currency>.Success(currency);
    }

    private static DomainResult<Currency> EnsureAmountIsNotNegative(decimal amount, Currency currency)
    {
        return amount >= 0
            ? DomainResult<Currency>.Success(currency)
            : DomainResult<Currency>.Failure(DomainError.Create(
                DomainErrorCodes.AmountNegative,
                "Amount cannot be negative."));
    }

    private static DomainResult<Currency> EnsureCurrencyIsPresentWhenAmountIsPositive(decimal amount, Currency currency)
    {
        return amount == 0 || !currency.IsEmpty
            ? DomainResult<Currency>.Success(currency)
            : DomainResult<Currency>.Failure(DomainError.Create(
                DomainErrorCodes.CurrencyRequired,
                "Currency is required when amount is greater than zero."));
    }

    private static DomainResult<Currency> EnsureAmountScaleIsValid(decimal amount, Currency currency)
    {
        return !HasMoreDecimalPlacesThan(amount, Scale)
            ? DomainResult<Currency>.Success(currency)
            : DomainResult<Currency>.Failure(DomainError.Create(
                DomainErrorCodes.AmountTooManyDecimalPlaces,
                $"Price cannot have more than {Scale} decimal places."));
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