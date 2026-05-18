namespace Company.Template.Application.Products.GetProducts;

/// <summary>
///     Coordinates the read workflow for retrieving a paged product list.
/// </summary>
/// <remarks>
///     Query composition and read-model projection stay behind the named query port so the use case does not load
///     aggregates or depend on EF Core query APIs directly.
/// </remarks>
public sealed class GetProductsUseCase : IUseCase<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductQueries _queries;

    public GetProductsUseCase(IProductQueries queries)
    {
        _queries = queries;
    }

    public async Task<Result<PagedResult<ProductDto>>> ExecuteAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        PagedResult<ProductDto> products = await _queries.GetProductsAsync(
            query.Filter,
            query.Sort,
            query.Page,
            cancellationToken);

        return Result<PagedResult<ProductDto>>.Success(products);
    }
}