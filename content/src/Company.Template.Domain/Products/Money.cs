namespace Company.Template.Domain.Products;

public sealed record Money
{
    public const int CurrencyMaxLength = 3;

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Price cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        var normalizedCurrency = currency.Trim().ToUpperInvariant();

        if (normalizedCurrency.Length != CurrencyMaxLength)
        {
            throw new ArgumentException("Currency must be an ISO 4217 three-letter code.", nameof(currency));
        }

        return new Money(amount, normalizedCurrency);
    }

    public static Money Zero(string currency)
    {
        return Create(0, currency);
    }
}
