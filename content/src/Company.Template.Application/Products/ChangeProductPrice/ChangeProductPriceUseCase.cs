using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.ChangeProductPrice;

public sealed class ChangeProductPriceUseCase
{
    private readonly IProductRepository _products;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public ChangeProductPriceUseCase(IProductRepository products, IUnitOfWork unitOfWork, IClock clock)
    {
        _products = products;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(ChangeProductPriceCommand command, CancellationToken cancellationToken)
    {
        if (command.ProductId == Guid.Empty)
        {
            return Result<ProductDto>.Failure(Error.Validation("Product id is required."));
        }

        if (command.Price < 0)
        {
            return Result<ProductDto>.Failure(Error.Validation("Price cannot be negative."));
        }

        var product = await _products.GetByIdAsync(ProductId.From(command.ProductId), cancellationToken);

        if (product is null)
        {
            return Result<ProductDto>.Failure(Error.NotFound("Product was not found."));
        }

        try
        {
            product.ChangePrice(Money.Create(command.Price, command.Currency), _clock.UtcNow);
        }
        catch (ArgumentException exception)
        {
            return Result<ProductDto>.Failure(Error.Validation(exception.Message));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(ProductMapper.ToDto(product));
    }
}
