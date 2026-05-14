using Company.Template.Application.Products.GetProducts;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products;

public interface IProductQueries : IQuery
{
    Task<Option<ProductDto>> GetByIdAsync(
        ProductId productId,
        CancellationToken cancellationToken);

    Task<PagedResult<ProductDto>> GetProductsAsync(
        ProductFilter filter,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken);
}
