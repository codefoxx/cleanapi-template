using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.CreateProduct;

public sealed class CreateProductUseCase
{
    private readonly IClock _clock;
    private readonly IProductDbContext _dbContext;

    public CreateProductUseCase(IProductDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result<ProductDto>.Failure(Error.Validation("Product name is required."));
        }

        if (command.Price < 0)
        {
            return Result<ProductDto>.Failure(Error.Validation("Price cannot be negative."));
        }

        Product product;

        try
        {
            product = Product.Create(
                ProductName.Create(command.Name),
                Money.Create(command.Price, command.Currency),
                _clock.UtcNow);
        }
        catch (ArgumentException exception)
        {
            return Result<ProductDto>.Failure(Error.Validation(exception.Message));
        }

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(ProductMapper.ToDto(product));
    }
}
