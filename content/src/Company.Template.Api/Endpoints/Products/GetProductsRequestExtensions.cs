using Company.Template.Application.Common;
using Company.Template.Application.Common.Validation;
using Company.Template.Application.Products.GetProducts;

namespace Company.Template.Api.Endpoints.Products;

internal static class GetProductsRequestExtensions
{
    public static Result<GetProductsQuery> ToQuery(this GetProductsRequest request)
    {
        return Validation
              .For(request)
              .Rule(ValidatePageRequest)
              .Rule(ValidateFilter)
              .Rule(ValidateSort)
              .Map(CreateQuery)
              .ToResult();
    }

    private static GetProductsQuery CreateQuery(GetProductsRequest request)
    {
        PageRequest page = PageRequest
                          .Create(request.PageNumber, request.PageSize)
                          .Value;

        ProductFilter filter = ProductFilter
                              .Create(
                                   request.Search,
                                   request.Status,
                                   request.Currency)
                              .Value;

        ProductSort sort = ProductSort
                          .Create(
                               request.SortBy,
                               request.SortDirection)
                          .Value;

        return new GetProductsQuery(page, filter, sort);
    }

    private static Error? ValidatePageRequest(GetProductsRequest request)
    {
        Result<PageRequest> result = PageRequest.Create(
            request.PageNumber,
            request.PageSize);

        return result.IsSuccess
            ? null
            : result.Error;
    }

    private static Error? ValidateFilter(GetProductsRequest request)
    {
        Result<ProductFilter> result = ProductFilter.Create(
            request.Search,
            request.Status,
            request.Currency);

        return result.IsSuccess
            ? null
            : result.Error;
    }

    private static Error? ValidateSort(GetProductsRequest request)
    {
        Result<ProductSort> result = ProductSort.Create(
            request.SortBy,
            request.SortDirection);

        return result.IsSuccess
            ? null
            : result.Error;
    }
}
