using Company.Template.Domain.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.CreateProduct;

/// <summary>
///     Coordinates the process of creating a new product.
/// </summary>
/// <remarks>
///     This use case handles request-level validation, delegates domain construction to
///     <see cref="Product" />, <see cref="ProductName" />, and <see cref="Money" />, and persists
///     the new aggregate. It returns a <see cref="Result{T}" /> which callers can translate
///     at their own boundary.
/// </remarks>
public sealed class CreateProductUseCase : IUseCase<CreateProductCommand, ProductDto>
{
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductUseCase(IUnitOfWork unitOfWork, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!Product.TryCreate(
                command.Name,
                command.Price,
                command.Currency,
                _clock.UtcNow,
                out Product? product,
                out DomainError? error))
        {
            return Result<ProductDto>.Failure(error.ToApplicationError());
        }

        IRepository<Product, ProductId> products = _unitOfWork.GetRepository<Product, ProductId>();
        products.Add(product);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(ProductMapper.ToDto(product));
    }
}
