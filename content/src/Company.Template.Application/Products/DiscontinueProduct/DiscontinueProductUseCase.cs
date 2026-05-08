using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.DiscontinueProduct;

public sealed class DiscontinueProductUseCase
{
    private readonly IProductRepository _products;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public DiscontinueProductUseCase(IProductRepository products, IUnitOfWork unitOfWork, IClock clock)
    {
        _products = products;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result> ExecuteAsync(DiscontinueProductCommand command, CancellationToken cancellationToken)
    {
        if (command.ProductId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("Product id is required."));
        }

        var product = await _products.GetByIdAsync(ProductId.From(command.ProductId), cancellationToken);

        if (product is null)
        {
            return Result.Failure(Error.NotFound("Product was not found."));
        }

        product.Discontinue(_clock.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
