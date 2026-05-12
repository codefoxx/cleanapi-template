using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.GetProductById;

/// <summary>
/// Coordinates the read workflow for retrieving a product snapshot by identifier.
/// </summary>
/// <remarks>
/// This use case keeps query validation and not-found handling at the application boundary while returning
/// a stable <see cref="ProductDto"/> instead of exposing the aggregate directly to callers.
/// </remarks>
public sealed class GetProductByIdUseCase : IUseCase<GetProductByIdQuery, ProductDto>
{
    private readonly IProductDbContext _dbContext;

    public GetProductByIdUseCase(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.ProductId == Guid.Empty)
        {
            return Result<ProductDto>.Failure(Error.Validation("Product id is required."));
        }

        var productId = ProductId.From(query.ProductId);

        Option<Product> maybe = await _dbContext.ProductsForRead
            .WithId(productId)
            .SingleOrNoneAsync(cancellationToken);

        return maybe.Match(
            some: product => Result<ProductDto>.Success(ProductMapper.ToDto(product)),
            none: () => Result<ProductDto>.Failure(Error.NotFound("Product was not found.")));
    }
}
