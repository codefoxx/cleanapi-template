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
    private readonly IProductQueries _queries;

    public GetProductByIdUseCase(IProductQueries queries)
    {
        _queries = queries;
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

        Option<ProductDto> product = await _queries.GetByIdAsync(productId, cancellationToken);

        return product.Match(MapToSuccess, ProductNotFound);
    }

    private static Result<ProductDto> MapToSuccess(ProductDto product)
    {
        return Result<ProductDto>.Success(product);
    }

    private static Result<ProductDto> ProductNotFound()
    {
        return Result<ProductDto>.Failure(Error.NotFound("Product was not found."));
    }
}
