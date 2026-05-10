using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.ChangeProductPrice;

public sealed class ChangeProductPriceUseCase : IUseCase<ChangeProductPriceCommand, ProductDto>
{
    private readonly IClock _clock;
    private readonly IProductDbContext _dbContext;


    public ChangeProductPriceUseCase(IProductDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(ChangeProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        if (command.ProductId == Guid.Empty)
        {
            return Result<ProductDto>.Failure(Error.Validation("Product id is required."));
        }

        if (command.Price < 0)
        {
            return Result<ProductDto>.Failure(Error.Validation("Price cannot be negative."));
        }

        var productId = ProductId.From(command.ProductId);
        Option<Product> maybe = await _dbContext.Products
                                                .WithId(productId)
                                                .SingleOrNoneAsync(cancellationToken);

        return await maybe.MatchAsync(
            some: async product =>
            {
                try
                {
                    product.ChangePrice(Money.Create(command.Price, command.Currency), _clock.UtcNow);
                }
                catch (ArgumentException exception)
                {
                    return Result<ProductDto>.Failure(Error.Validation(exception.Message));
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result<ProductDto>.Success(ProductMapper.ToDto(product));
            },
            none: () => Result<ProductDto>.Failure(Error.NotFound("Product was not found.")));
    }
}
