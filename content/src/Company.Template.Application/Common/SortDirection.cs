namespace Company.Template.Application.Common;

public enum SortDirection
{
    Ascending,
    Descending
}

public static class SortDirectionValue
{
    public const string AllowedValues = "ascending, descending";

    public static bool TryParse(string? value, out SortDirection direction)
    {
        direction = SortDirection.Descending;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return value.Trim().ToUpperInvariant() switch
        {
            "ASCENDING" or "ASC" => Set(SortDirection.Ascending, out direction),
            "DESCENDING" or "DESC" => Set(SortDirection.Descending, out direction),
            _ => false
        };
    }

    private static bool Set(SortDirection value, out SortDirection direction)
    {
        direction = value;
        return true;
    }
}
