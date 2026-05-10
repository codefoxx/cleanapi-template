using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.GetProductById;

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
