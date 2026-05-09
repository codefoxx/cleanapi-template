using System;

namespace Company.Template.Domain.Products;

public sealed record ProductName
{
    public const int MaxLength = 200;

    private ProductName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ProductName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Product name is required.", nameof(value));
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Product name cannot exceed {MaxLength} characters.", nameof(value));
        }

        return new ProductName(trimmed);
    }

    public override string ToString()
    {
        return Value;
    }
}
