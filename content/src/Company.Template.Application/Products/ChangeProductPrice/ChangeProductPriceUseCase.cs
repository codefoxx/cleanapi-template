using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.ChangeProductPrice;

/// <summary>
///     Coordinates the workflow for changing a product price.
/// </summary>
/// <remarks>
///     The use case performs request-level validation, loads the target aggregate through the EF Core-shaped
///     application boundary, delegates price validation and state changes to <see cref="Money" /> and
///     <see cref="Product" />, and returns explicit validation or not-found failures for expected outcomes.
/// </remarks>
public sealed class ChangeProductPriceUseCase : IUseCase<ChangeProductPriceCommand, ProductDto>
{
    private readonly IClock _clock;
    private readonly IProductDbContext _dbContext;

    public ChangeProductPriceUseCase(IProductDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(
        ChangeProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!ProductId.TryFrom(command.ProductId, out ProductId productId, out DomainError? productIdError))
        {
            return Result<ProductDto>.Failure(productIdError.ToApplicationError());
        }

        if (!Money.TryCreate(command.Price, command.Currency, out Money? money, out DomainError? moneyError))
        {
            return Result<ProductDto>.Failure(moneyError.ToApplicationError());
        }

        Option<Product> maybe = await _dbContext.Products
                                                .WithId(productId)
                                                .SingleOrNoneAsync(cancellationToken);

        if (!maybe.TryGetValue(out Product? product))
        {
            return Result<ProductDto>.Failure(Error.NotFound("Product was not found."));
        }

        product.ChangePrice(money, _clock.UtcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(ProductMapper.ToDto(product));
    }
}
