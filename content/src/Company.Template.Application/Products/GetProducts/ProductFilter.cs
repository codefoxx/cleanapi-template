using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.GetProducts;

/// <summary>
///     Validated and normalized product query filters.
/// </summary>
/// <remarks>
///     Optional values mean that the corresponding filter should not be applied.
///     Raw HTTP query-string values should be translated through <see cref="Create" /> before this type is used by
///     queries.
/// </remarks>
public sealed record ProductFilter(
    Option<string> Search,
    Option<ProductStatus> Status,
    Option<string> Currency)
{
    public static Result<ProductFilter> Create(
        string? search,
        string? status,
        string? currency)
    {
        Result<Option<ProductStatus>> statusResult = ParseStatus(status);

        if (!statusResult.IsSuccess)
        {
            return Result<ProductFilter>.Failure(statusResult.Error!);
        }

        ProductFilter filter = new(
            NormalizeSearch(search),
            statusResult.Value,
            NormalizeCurrency(currency));

        return Result<ProductFilter>.Success(filter);
    }

    private static Result<Option<ProductStatus>> ParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<Option<ProductStatus>>.Success(Option.None<ProductStatus>());
        }

        string normalizedValue = value.Trim().ToUpperInvariant();

        return normalizedValue switch
        {
            "DRAFT" => Result<Option<ProductStatus>>.Success(Option.Some(ProductStatus.Draft)),
            "ACTIVE" => Result<Option<ProductStatus>>.Success(Option.Some(ProductStatus.Active)),
            "DISCONTINUED" => Result<Option<ProductStatus>>.Success(Option.Some(ProductStatus.Discontinued)),

            _ => Result<Option<ProductStatus>>.Failure(
                Error.Validation("Status must be one of: draft, active, discontinued."))
        };
    }

    private static Option<string> NormalizeSearch(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Option.None<string>()
            : Option.Some(value.Trim());
    }

    private static Option<string> NormalizeCurrency(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Option.None<string>()
            : Option.Some(value.Trim().ToUpperInvariant());
    }
}
