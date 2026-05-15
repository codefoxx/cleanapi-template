using Company.Template.Domain.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.ChangeProductPrice;

/// <summary>
///     Coordinates the workflow for changing a product price.
/// </summary>
/// <remarks>
///     The use case performs request-level validation, loads the target aggregate through the command persistence
///     boundary,
///     delegates price validation and state changes to <see cref="Money" /> and <see cref="Product" />,
///     and returns explicit validation or not-found failures for expected outcomes.
/// </remarks>
public sealed class ChangeProductPriceUseCase : IUseCase<ChangeProductPriceCommand, ProductDto>
{
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeProductPriceUseCase(IUnitOfWork unitOfWork, IClock clock)
    {
        _unitOfWork = unitOfWork;
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

        IRepository<Product, ProductId> products = _unitOfWork.GetRepository<Product, ProductId>();

        Option<Product> maybe = await products.FindAsync(productId, cancellationToken);

        if (!maybe.TryGetValue(out Product? product))
        {
            return Result<ProductDto>.Failure(Error.NotFound("Product was not found."));
        }

        DomainResult result = product.ChangePrice(money, _clock.UtcNow);

        if (result.IsFailure)
        {
            return Result<ProductDto>.Failure(result.Error.ToApplicationError());
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(ProductMapper.ToDto(product));
    }
}
