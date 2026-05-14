namespace Company.Template.Application.Products.GetProducts;

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
