using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.DiscontinueProduct;

/// <summary>
/// Coordinates the workflow for discontinuing a product.
/// </summary>
/// <remarks>
/// The use case validates the request identifier, loads the aggregate through the application persistence boundary,
/// and asks the domain model to apply lifecycle rules. Expected validation and not-found outcomes are returned as
/// explicit results rather than exceptions.
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
        if (command.ProductId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("Product id is required."));
        }

        var productId = ProductId.From(command.ProductId);
        Option<Product> maybe = await _dbContext.Products
            .WithId(productId)
            .SingleOrNoneAsync(cancellationToken);

        return await maybe.MatchAsync(
            some: async product =>
            {
                product.Discontinue(_clock.UtcNow);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result.Success();
            },
            none: () => Result.Failure(Error.NotFound("Product was not found.")));
    }
}
