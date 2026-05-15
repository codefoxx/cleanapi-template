using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Application.Products.DiscontinueProduct;
using Company.Template.Domain.Common;
using Company.Template.Domain.Products;
using Company.Template.Infrastructure.Persistence;

namespace Company.Template.Application.Tests.Products.DiscontinueProduct;

public sealed class DiscontinueProductUseCaseTests : IClassFixture<TestDatabase>
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DiscontinuedAt = new(2026, 1, 2, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset LaterDiscontinuedAt = new(2026, 1, 3, 10, 0, 0, TimeSpan.Zero);

    private readonly TestDatabase _database;

    public DiscontinueProductUseCaseTests(TestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingProduct_ReturnsSuccess()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync();

        ProductDto product = await CreateProductAsync(dbContext);

        DiscontinueProductUseCase useCase = new(
            dbContext,
            new FixedClock(DiscontinuedAt));

        DiscontinueProductCommand command = new(product.Id);

        // Act
        Result result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.IsNone.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingProduct_PersistsDiscontinuedProduct()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync();

        ProductDto product = await CreateProductAsync(dbContext);

        DiscontinueProductUseCase useCase = new(
            dbContext,
            new FixedClock(DiscontinuedAt));

        DiscontinueProductCommand command = new(product.Id);

        // Act
        Result result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync(x => x.Id == ProductId.From(product.Id));

        persistedProduct.Status.ShouldBe(ProductStatus.Discontinued);
        persistedProduct.CreatedAt.ShouldBe(CreatedAt);
        persistedProduct.DiscontinuedAt.ShouldBe(DiscontinuedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingProduct_DispatchesProductDiscontinuedDomainEvent()
    {
        // Arrange
        RecordingDomainEventDispatcher domainEventDispatcher = new();

        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync(domainEventDispatcher);

        ProductDto product = await CreateProductAsync(dbContext);

        DiscontinueProductUseCase useCase = new(
            dbContext,
            new FixedClock(DiscontinuedAt));

        DiscontinueProductCommand command = new(product.Id);


        // Act
        Result result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        ProductDiscontinuedDomainEvent domainEvent = domainEventDispatcher.DispatchedEvents
                                                                          .OfType<ProductDiscontinuedDomainEvent>()
                                                                          .Single();

        domainEvent.ProductId.Value.ShouldBe(product.Id);
        domainEvent.OccurredAt.ShouldBe(DiscontinuedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyDiscontinuedProduct_ReturnsSuccess()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync();

        ProductDto product = await CreateProductAsync(dbContext);

        DiscontinueProductUseCase firstUseCase = new(
            dbContext,
            new FixedClock(DiscontinuedAt));

        Result firstResult = await firstUseCase.ExecuteAsync(
            new DiscontinueProductCommand(product.Id),
            CancellationToken.None);

        firstResult.IsSuccess.ShouldBeTrue();

        DiscontinueProductUseCase secondUseCase = new(
            dbContext,
            new FixedClock(LaterDiscontinuedAt));

        DiscontinueProductCommand command = new(product.Id);

        // Act
        Result result = await secondUseCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.IsNone.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyDiscontinuedProduct_KeepsOriginalDiscontinuedAt()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync();

        ProductDto product = await CreateProductAsync(dbContext);

        DiscontinueProductUseCase firstUseCase = new(
            dbContext,
            new FixedClock(DiscontinuedAt));

        Result firstResult = await firstUseCase.ExecuteAsync(
            new DiscontinueProductCommand(product.Id),
            CancellationToken.None);

        firstResult.IsSuccess.ShouldBeTrue();

        DiscontinueProductUseCase secondUseCase = new(
            dbContext,
            new FixedClock(LaterDiscontinuedAt));

        DiscontinueProductCommand command = new(product.Id);

        // Act
        Result result = await secondUseCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync(x => x.Id == ProductId.From(product.Id));

        persistedProduct.Status.ShouldBe(ProductStatus.Discontinued);
        persistedProduct.DiscontinuedAt.ShouldBe(DiscontinuedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProductId_ReturnsNotFoundFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync();

        DiscontinueProductUseCase useCase = new(
            dbContext,
            new FixedClock(DiscontinuedAt));

        DiscontinueProductCommand command = new(Guid.NewGuid());

        // Act
        Result result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        Error error = AssertError(result, ErrorType.NotFound);

        error.Code.ShouldBe("not_found");
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyProductId_ReturnsValidationFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync();

        DiscontinueProductUseCase useCase = new(
            dbContext,
            new FixedClock(DiscontinuedAt));

        DiscontinueProductCommand command = new(Guid.Empty);

        // Act
        Result result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        AssertError(result, ErrorType.Validation, DomainErrorCodes.ProductIdRequired);
    }

    private static async Task<ProductDto> CreateProductAsync(ApplicationDbContext dbContext)
    {
        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(CreatedAt));

        CreateProductCommand command = new(
            "Keyboard",
            99.90m,
            "CHF");

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }

    private static Error AssertError(
        Result result,
        ErrorType expectedType)
    {
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();

        Error error = result.Error;

        error.ShouldNotBe(Error.None);
        error.Type.ShouldBe(expectedType);

        return error;
    }

    private static Error AssertError(
        Result result,
        ErrorType expectedType,
        DomainErrorCode expectedCode)
    {
        Error error = AssertError(result, expectedType);

        error.Code.ShouldBe(expectedCode.Value);

        return error;
    }
}
