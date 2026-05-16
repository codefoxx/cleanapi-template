namespace Company.Template.Domain.SharedKernel;

/// <summary>
///     Contains the ISO 4217 alphabetic currency codes supported by this application.
/// </summary>
/// <remarks>
///     This list is intentionally application-owned. Add or remove codes here when
///     the domain needs to support additional currencies.
/// </remarks>
public static class Iso4217CurrencyCodes
{
    private static readonly HashSet<string> ValidCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "CHF",
        "EUR",
        "USD"
    };

    public static bool Contains(string? code)
    {
        return !string.IsNullOrWhiteSpace(code)
         && ValidCodes.Contains(code.Trim());
    }

    public static Currency Chf { get; } = Currency.Create("CHF", "CHF");
    public static Currency Eur { get; } = Currency.Create("EUR", "€");
    public static Currency Usd { get; } = Currency.Create("USD", "$");
}
