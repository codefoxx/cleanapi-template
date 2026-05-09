using System.Globalization;

namespace Company.Template.Domain.Products;

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

    public static Money CreateRounded(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        var roundedAmount = decimal.Round(
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
