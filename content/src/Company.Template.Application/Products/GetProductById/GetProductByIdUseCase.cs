using Company.Template.Domain.Common;
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

        if (!ProductId.TryFrom(query.ProductId, out ProductId productId, out DomainError? productIdError))
        {
            return Result<ProductDto>.Failure(productIdError.ToApplicationError());
        }

        Option<Product> product = await _dbContext.ProductsForRead
                                                  .WithId(productId)
                                                  .SingleOrNoneAsync(cancellationToken);

        return product.Match(MapToSuccess, ProductNotFound);
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
