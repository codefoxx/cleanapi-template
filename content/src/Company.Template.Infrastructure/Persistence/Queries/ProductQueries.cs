using System.Linq.Expressions;
using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.GetProducts;
using Company.Template.Domain.Products;

namespace Company.Template.Infrastructure.Persistence.Queries;

public sealed class ProductQueries : IProductQueries
{
    private readonly ApplicationDbContext _dbContext;

    public ProductQueries(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Option<ProductDto>> GetByIdAsync(
        ProductId productId,
        CancellationToken cancellationToken)
    {
        ProductDto? product = await _dbContext.Products
                                              .AsNoTracking()
                                              .WithId(productId)
                                              .Select(ToDtoProjection())
                                              .SingleOrDefaultAsync(cancellationToken);

        return Option.FromNullable(product);
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(
        ProductFilter filter,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken)
    {
        IQueryable<Product> query = _dbContext.Products
                                              .AsNoTracking()
                                              .WithDefaultVisibility(filter)
                                              .WithFilter(filter);

        int totalCount = await query.CountAsync(cancellationToken);

        ProductDto[] items = await query.WithSorting(sort)
                                        .Skip(page.Skip)
                                        .Take(page.PageSize)
                                        .Select(ToDtoProjection())
                                        .ToArrayAsync(cancellationToken);

        return new PagedResult<ProductDto>(
            items,
            page.PageNumber,
            page.PageSize,
            totalCount);
    }

    private static Expression<Func<Product, ProductDto>> ToDtoProjection()
    {
        return product => new ProductDto(
            product.Id.Value,
            product.Name.Value,
            product.Price,
            product.Status,
            product.CreatedAt,
            product.DiscontinuedAt);
    }
}
