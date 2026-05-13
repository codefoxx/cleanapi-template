namespace Company.Template.Domain.Products;

/// <summary>
///     Provides a registry of common currencies to avoid repeated string literals,
///     while still allowing arbitrary <see cref="Currency" /> creation when needed.
/// </summary>
public static class KnownCurrencies
{
    public static Currency Chf { get; } = Currency.Create("CHF", "CHF");
    public static Currency Eur { get; } = Currency.Create("EUR", "€");
    public static Currency Usd { get; } = Currency.Create("USD", "$");
}
