using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.DiscontinueProduct;

/// <summary>
///     Coordinates the workflow for discontinuing a product.
/// </summary>
/// <remarks>
///     The use case validates the request identifier, loads the aggregate through the application persistence boundary,
///     and asks the domain model to apply lifecycle rules. Expected validation and not-found outcomes are returned as
///     explicit results rather than exceptions.
/// </remarks>
public sealed class DiscontinueProductUseCase : IUseCase<DiscontinueProductCommand>
{
    private readonly IClock _clock;
    private readonly IProductDbContext _dbContext;

    public DiscontinueProductUseCase(IProductDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result> ExecuteAsync(DiscontinueProductCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!ProductId.TryFrom(command.ProductId, out ProductId productId, out DomainError? productIdError))
        {
            return Result.Failure(productIdError.ToApplicationError());
        }

        Option<Product> maybe = await _dbContext.Products
                                                .WithId(productId)
                                                .SingleOrNoneAsync(cancellationToken);

        if (!maybe.TryGetValue(out Product? product))
        {
            return Result.Failure(Error.NotFound("Product was not found."));
        }

        product.Discontinue(_clock.UtcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
