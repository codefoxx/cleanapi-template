namespace Company.Template.Application.Products.GetProducts;

public sealed record ProductSort(
    ProductSortField Field,
    SortDirection Direction)
{
    public static ProductSort Default => new(
        ProductSortField.Default,
        SortDirection.Descending);

    public static Result<ProductSort> Create(string? sortBy, string? sortDirection)
    {
        if (!ProductSortField.TryParse(sortBy, out ProductSortField field))
        {
            return Result<ProductSort>.Failure(
                Error.Validation($"SortBy must be one of: {ProductSortField.AllowedValues}."));
        }

        if (!SortDirectionValue.TryParse(sortDirection, out SortDirection direction))
        {
            return Result<ProductSort>.Failure(
                Error.Validation($"SortDirection must be one of: {SortDirectionValue.AllowedValues}."));
        }

        return Result<ProductSort>.Success(new ProductSort(field, direction));
    }
}
