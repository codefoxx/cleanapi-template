using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.GetProductById;

/// <summary>
///     Coordinates the read workflow for retrieving a product snapshot by identifier.
/// </summary>
/// <remarks>
///     This use case keeps query validation and not-found handling at the application boundary while returning
///     a stable <see cref="ProductDto" /> instead of exposing the aggregate directly to callers.
/// </remarks>
public sealed class GetProductByIdUseCase : IUseCase<GetProductByIdQuery, ProductDto>
{
    private readonly IProductDbContext _dbContext;

    public GetProductByIdUseCase(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(
        GetProductByIdQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.ProductId == Guid.Empty)
        {
            return Result<ProductDto>.Failure(Error.Validation("Product id is required."));
        }

        ProductId productId = ProductId.From(query.ProductId);

        Option<Product> product = await _dbContext.ProductsForRead
                                                  .WithId(productId)
                                                  .SingleOrNoneAsync(cancellationToken);

        return product.Match(
            MapToSuccess,
            ProductNotFound);
    }

    private static Result<ProductDto> MapToSuccess(Product product)
    {
        return Result<ProductDto>.Success(ProductMapper.ToDto(product));
    }

    private static Result<ProductDto> ProductNotFound()
    {
        return Result<ProductDto>.Failure(Error.NotFound("Product was not found."));
    }
}
