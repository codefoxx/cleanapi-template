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
    private readonly IUnitOfWork _unitOfWork;

    public DiscontinueProductUseCase(IUnitOfWork unitOfWork, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result> ExecuteAsync(
        DiscontinueProductCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!ProductId.TryFrom(command.ProductId, out ProductId productId, out DomainError? productIdError))
        {
            return Result.Failure(productIdError.ToApplicationError());
        }

        IRepository<Product, ProductId> products = _unitOfWork.GetRepository<Product, ProductId>();

        Option<Product> maybe = await products.TryFindAsync(productId, cancellationToken);

        if (!maybe.TryGetValue(out Product? product))
        {
            return Result.Failure(Error.NotFound("Product was not found."));
        }

        DomainResult result = product.Discontinue(_clock.UtcNow);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error.ToApplicationError());
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
