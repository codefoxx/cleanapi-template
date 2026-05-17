using Company.Template.Domain.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.CreateProduct;

/// <summary>
///     Coordinates product creation by delegating invariant checks to the domain model and persisting the aggregate.
/// </summary>
/// <remarks>
///     The API boundary validates request shape before command creation. The use case still uses safe domain APIs as the
///     final guard because commands are application input, not domain objects.
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