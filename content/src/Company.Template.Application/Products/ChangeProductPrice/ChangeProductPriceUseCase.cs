using Company.Template.Application.Abstractions;
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

        if (command.ProductId == Guid.Empty)
        {
            return Result<ProductDto>.Failure(Error.Validation("Product id is required."));
        }

        if (command.Price < 0)
        {
            return Result<ProductDto>.Failure(Error.Validation("Price cannot be negative."));
        }

        ProductId productId = ProductId.From(command.ProductId);

        Option<Product> maybe = await _dbContext.Products
                                                .WithId(productId)
                                                .SingleOrNoneAsync(cancellationToken);

        if (!maybe.TryGetValue(out Product? product))
        {
            return Result<ProductDto>.Failure(Error.NotFound("Product was not found."));
        }

        try
        {
            product.ChangePrice(
                Money.Create(command.Price, command.Currency),
                _clock.UtcNow);
        }
        catch (ArgumentException exception)
        {
            return Result<ProductDto>.Failure(Error.Validation(exception.Message));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(ProductMapper.ToDto(product));
    }
}
