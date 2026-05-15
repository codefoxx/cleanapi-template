using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.GetProductById;
using Company.Template.Domain.Common;
using Company.Template.Domain.Products;
using Company.Template.Infrastructure.Persistence;
using Company.Template.Infrastructure.Persistence.Queries;

namespace Company.Template.Application.Tests.Products.GetProductById;

public sealed class GetProductByIdUseCaseTests : IClassFixture<TestDatabase>
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DiscontinuedAt = new(2026, 1, 3, 10, 0, 0, TimeSpan.Zero);

    private readonly TestDatabase _database;

    public GetProductByIdUseCaseTests(TestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingProduct_ReturnsSuccess()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();
        Product product = CreateProduct();

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        GetProductByIdUseCase useCase = CreateUseCase(dbContext);
        GetProductByIdQuery query = new(product.Id.Value);

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        ShouldBeMechanicalKeyboard(result.Value, product.Id.Value);
        result.Value.Status.ShouldBe(ProductStatus.Active);
        result.Value.CreatedAt.ShouldBe(CreatedAt);
        result.Value.DiscontinuedAt.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithDiscontinuedProduct_ReturnsProductSnapshot()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();
        Product product = CreateProduct();

        product.Discontinue(DiscontinuedAt);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        GetProductByIdUseCase useCase = CreateUseCase(dbContext);
        GetProductByIdQuery query = new(product.Id.Value);

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        ShouldBeMechanicalKeyboard(result.Value, product.Id.Value);
        result.Value.Status.ShouldBe(ProductStatus.Discontinued);
        result.Value.CreatedAt.ShouldBe(CreatedAt);
        result.Value.DiscontinuedAt.ShouldBe(DiscontinuedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProductId_ReturnsNotFoundFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();
        GetProductByIdUseCase useCase = CreateUseCase(dbContext);
        GetProductByIdQuery query = new(Guid.NewGuid());

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyProductId_ReturnsValidationFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();
        GetProductByIdUseCase useCase = CreateUseCase(dbContext);
        GetProductByIdQuery query = new(Guid.Empty);

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Type.ShouldBe(ErrorType.Validation);
        result.Error.Code.ShouldBe(DomainErrorCodes.ProductIdRequired.Value);
    }

    private static Product CreateProduct()
    {
        return Product.Create(
            ProductName.Create("Mechanical Keyboard"),
            Money.Create(199.90m, Currency.Create("CHF")),
            CreatedAt);
    }

    private static GetProductByIdUseCase CreateUseCase(ApplicationDbContext dbContext)
    {
        return new GetProductByIdUseCase(new ProductQueries(dbContext));
    }

    private static void ShouldBeMechanicalKeyboard(ProductDto product, Guid expectedId)
    {
        product.Id.ShouldBe(expectedId);
        product.Name.ShouldBe("Mechanical Keyboard");
        product.Price.Amount.ShouldBe(199.90m);
        product.Price.Currency.Code.ShouldBe("CHF");
    }
}
