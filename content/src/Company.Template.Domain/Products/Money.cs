namespace Company.Template.Domain.Products;

/// <summary>
///     Represents a monetary value with a specific currency.
/// </summary>
/// <remarks>
///     <see cref="Money" /> is a value object that ensures mathematical consistency and enforces rules
///     such as matching currencies for operations and rounding to a standard scale.
/// </remarks>
public sealed record Money : IComparable<Money>
{
    /// <summary>The standard number of decimal places for monetary values.</summary>
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

    /// <summary>
    ///     Creates a new <see cref="Money" /> instance with validation.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency.</param>
    /// <returns>A new <see cref="Money" /> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is negative.</exception>
    /// <exception cref="ArgumentException">Thrown when currency is missing for non-zero amounts or scale is exceeded.</exception>
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

    public static Money Create(decimal amount, string currency)
    {
        return Create(amount, Currency.Create(currency));
    }

    /// <summary>Creates a new <see cref="Money" /> instance, rounding the amount to the standard scale.</summary>
    public static Money CreateRounded(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        decimal roundedAmount = decimal.Round(
            amount,
            Scale,
            MidpointRounding.AwayFromZero);

        return Create(roundedAmount, currency);
    }

    public static Money Zero()
    {
        return Zero(Currency.Empty);
    }

    public static Money Zero(Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        return new Money(0, currency);
    }

    /// <exception cref="InvalidOperationException">Thrown when currencies do not match.</exception>
    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        EnsureSameCurrency(other);

        return Create(Amount + other.Amount, Currency);
    }

    /// <exception cref="InvalidOperationException">Thrown when currencies do not match or the result would be negative.</exception>
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

    /// <summary>Scales the money value by a factor, rounding the result.</summary>
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
