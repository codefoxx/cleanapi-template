using Company.Template.Domain.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.ChangeProductPrice;

/// <summary>
///     Coordinates the workflow for changing a product price.
/// </summary>
/// <remarks>
///     The API boundary validates request shape before command creation. The use case resolves domain types, loads the
///     aggregate through the command persistence boundary, delegates lifecycle checks to the domain model, and returns
///     explicit results for expected validation, not-found, or conflict outcomes.
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

        return await maybe.Match(
            some: product => ChangePriceAsync(product, money, cancellationToken),
            none: () => Task.FromResult(ProductNotFound()));
    }

    private async Task<Result<ProductDto>> ChangePriceAsync(
        Product product,
        Money money,
        CancellationToken cancellationToken)
    {
        DomainResult result = product.ChangePrice(money, _clock.UtcNow);

        return await result.Match(
            success: async () =>
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<ProductDto>.Success(ProductMapper.ToDto(product));
            },
            failure: error => Task.FromResult(Result<ProductDto>.Failure(error.ToApplicationError())));
    }

    private static Result<ProductDto> ProductNotFound()
    {
        return Result<ProductDto>.Failure(Error.NotFound("Product was not found."));
    }
}
