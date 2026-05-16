namespace Company.Template.Application.Products.GetProducts;

public sealed class ProductSortField
{
    static ProductSortField()
    {
        CreatedAt = new ProductSortField(
            "createdAt",
            "created-at",
            "created_at");

        Name = new ProductSortField("name");
        Price = new ProductSortField("price");
        Status = new ProductSortField("status");

        All =
        [
            Name,
            Price,
            CreatedAt,
            Status
        ];
    }

    private ProductSortField(string value, params string[] aliases)
    {
        Value = value;
        Aliases = aliases;
    }

    public static IReadOnlyList<ProductSortField> All { get; }

    public static string AllowedValues => string.Join(
        ", ",
        All.Select<ProductSortField, string>(sortField => sortField.Value));

    public static ProductSortField CreatedAt { get; }

    public static ProductSortField Default => CreatedAt;

    public static ProductSortField Name { get; }

    public static ProductSortField Price { get; }

    public static ProductSortField Status { get; }

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
