namespace Company.Template.Application.Products.GetProducts;

public sealed class ProductSortField
{
    private ProductSortField(string value, params string[] aliases)
    {
        Value = value;
        Aliases = aliases;
    }

    public static IReadOnlyList<ProductSortField> All { get; } =
    [
        Name,
        Price,
        CreatedAt,
        Status
    ];

    public static string AllowedValues => string.Join(", ", All.Select<ProductSortField, string>(@field => @field.Value));

    public static ProductSortField CreatedAt { get; } = new(
        "createdAt",
        "created-at",
        "created_at");

    public static ProductSortField Default => CreatedAt;

    public static ProductSortField Name { get; } = new("name");

    public static ProductSortField Price { get; } = new("price");

    public static ProductSortField Status { get; } = new("status");

    public IReadOnlyCollection<string> Aliases { get; }

    public string Value { get; }

    public static bool TryParse(string? value, out ProductSortField field)
    {
        field = Default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        ProductSortField? match = All.SingleOrDefault(candidate => candidate.Matches(value));

        if (match is null)
        {
            return false;
        }

        field = match;
        return true;
    }

    private bool Matches(string value)
    {
        string normalizedValue = value.Trim();

        return string.Equals(Value, normalizedValue, StringComparison.OrdinalIgnoreCase)
         || Aliases.Any(alias => string.Equals(alias, normalizedValue, StringComparison.OrdinalIgnoreCase));
    }
}
