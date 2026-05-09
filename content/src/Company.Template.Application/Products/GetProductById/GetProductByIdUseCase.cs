using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.GetProductById;

public sealed class GetProductByIdUseCase
{
    private readonly IProductRepository _products;

    public GetProductByIdUseCase(IProductRepository products)
    {
        _products = products;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.ProductId == Guid.Empty)
        {
            return Result<ProductDto>.Failure(Error.Validation("Product id is required."));
        }

        Product? product = await _products.GetByIdAsync(ProductId.From(query.ProductId), cancellationToken);

        return product is null
            ? Result<ProductDto>.Failure(Error.NotFound("Product was not found."))
            : Result<ProductDto>.Success(ProductMapper.ToDto(product));
    }
}
