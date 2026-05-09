namespace Company.Template.Domain.Products;

public static class KnownCurrencies
{
    public static Currency Chf { get; } = Currency.Create("CHF", "CHF");
    public static Currency Eur { get; } = Currency.Create("EUR", "€");
    public static Currency Usd { get; } = Currency.Create("USD", "$");
}
