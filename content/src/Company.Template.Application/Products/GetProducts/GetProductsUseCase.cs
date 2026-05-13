using Company.Template.Application.Abstractions;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.GetProducts;

public sealed class GetProductsUseCase : IUseCase<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductDbContext _dbContext;

    public GetProductsUseCase(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<ProductDto>>> ExecuteAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<Product> products = _dbContext.ProductsForRead
                                                 .WithFilter(query.Filter);

        int totalCount = await products.CountAsync(cancellationToken);

        List<ProductDto> items = await products
                                      .WithSorting(query.Sort)
                                      .Skip(query.Page.Skip)
                                      .Take(query.Page.PageSize)
                                      .Select(ProductMapper.ToDtoExpression)
                                      .ToListAsync(cancellationToken);

        return Result<PagedResult<ProductDto>>.Success(
            new PagedResult<ProductDto>(
                items,
                query.Page.PageNumber,
                query.Page.PageSize,
                totalCount));
    }
}
