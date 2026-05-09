using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

public sealed record Currency
{
    public const int CodeLength = 3;

    private Currency(string code, string symbol)
    {
        Code = code;
        Symbol = symbol;
    }

    public string Code { get; }

    public string Symbol { get; }

    public static Currency Empty { get; } = new(string.Empty, string.Empty);

    public bool IsEmpty => string.IsNullOrEmpty(Code);

    public static Currency Create(string code)
    {
        Guard.ThrowIfNullOrWhiteSpace(code, nameof(code), "Currency is required.");

        var normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length != CodeLength)
        {
            throw new ArgumentException("Currency must be an ISO 4217 three-letter code.", nameof(code));
        }

        return new Currency(normalizedCode, normalizedCode);
    }

    public static Currency Create(string code, string symbol)
    {
        Guard.ThrowIfNullOrWhiteSpace(code, nameof(code), "Currency is required.");
        Guard.ThrowIfNullOrWhiteSpace(symbol, nameof(symbol), "Currency symbol is required.");

        var normalizedCode = code.Trim().ToUpperInvariant();
        var normalizedSymbol = symbol.Trim();

        if (normalizedCode.Length != CodeLength)
        {
            throw new ArgumentException("Currency must be an ISO 4217 three-letter code.", nameof(code));
        }

        return new Currency(normalizedCode, normalizedSymbol);
    }

    public override string ToString()
    {
        return Code;
    }
}
