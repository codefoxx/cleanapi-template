using Company.Template.Domain.Common;
using Company.Template.Domain.Products;
using ProductCurrency = Company.Template.Domain.SharedKernel.Currency;

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
    Option<ProductCurrency> Currency)
{
    public static Result<ProductFilter> Create(
        string? search,
        string? status,
        string? currency)
    {
        Result<Option<ProductStatus>> statusResult = ParseStatus(status);

        if (!statusResult.IsSuccess)
        {
            return Result<ProductFilter>.Failure(statusResult.Error);
        }

        Result<Option<ProductCurrency>> currencyResult = ParseCurrency(currency);

        if (!currencyResult.IsSuccess)
        {
            return Result<ProductFilter>.Failure(currencyResult.Error);
        }

        ProductFilter filter = new(
            NormalizeSearch(search),
            statusResult.Value,
            currencyResult.Value);

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

    private static Result<Option<ProductCurrency>> ParseCurrency(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<Option<ProductCurrency>>.Success(Option.None<ProductCurrency>());
        }

        if (!ProductCurrency.TryCreate(value, out ProductCurrency? parsedCurrency, out DomainError? error))
        {
            return Result<Option<ProductCurrency>>.Failure(error.ToApplicationError());
        }

        return Result<Option<ProductCurrency>>.Success(Option.Some(parsedCurrency));
    }

    private static Option<string> NormalizeSearch(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Option.None<string>()
            : Option.Some(value.Trim());
    }
}
