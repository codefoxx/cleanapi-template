using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Application.Tests.TestSupport;
using Company.Template.Domain.Products;
using Company.Template.Infrastructure.Persistence;

namespace Company.Template.Application.Tests.Products.CreateProduct;

public sealed class CreateProductUseCaseTests : IClassFixture<TestDatabase>
{
    private static readonly DateTimeOffset UtcNow = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly TestDatabase _database;

    public CreateProductUseCaseTests(TestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(UtcNow));

        CreateProductCommand command = new(
            "Keyboard",
            99.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();

        ProductDto product = result.Value;

        product.Id.ShouldNotBe(Guid.Empty);
        product.Name.ShouldBe("Keyboard");
        product.Price.ShouldBe(Money.Create(99.90m, KnownCurrencies.Chf));
        product.Status.ShouldBe(ProductStatus.Active);
        product.CreatedAt.ShouldBe(UtcNow);
        product.DiscontinuedAt.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_PersistsProduct()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(UtcNow));

        CreateProductCommand command = new(
            "Keyboard",
            99.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        ProductDto product = result.Value;

        Product persistedProduct = await dbContext.ProductsForRead.SingleAsync();

        persistedProduct.Id.Value.ShouldBe(product.Id);
        persistedProduct.Name.ShouldBe(ProductName.Create("Keyboard"));
        persistedProduct.Price.ShouldBe(Money.Create(99.90m, KnownCurrencies.Chf));
        persistedProduct.Status.ShouldBe(ProductStatus.Active);
        persistedProduct.CreatedAt.ShouldBe(UtcNow);
        persistedProduct.DiscontinuedAt.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_DispatchesProductCreatedDomainEvent()
    {
        // Arrange
        RecordingDomainEventDispatcher domainEventDispatcher = new();

        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync(domainEventDispatcher);

        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(UtcNow));

        CreateProductCommand command = new(
            "Keyboard",
            99.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        ProductDto product = result.Value;

        ProductCreatedDomainEvent domainEvent = domainEventDispatcher.DispatchedEvents
                                                                     .Single()
                                                                     .ShouldBeOfType<ProductCreatedDomainEvent>();

        domainEvent.ProductId.Value.ShouldBe(product.Id);
        domainEvent.OccurredAt.ShouldBe(UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task ExecuteAsync_WithMissingName_ReturnsValidationFailure(string name)
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(UtcNow));

        CreateProductCommand command = new(
            name,
            99.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        Error error = ShouldBeValidationFailure(result);
        error.Message.ShouldBe("Product name is required.");

        bool productWasPersisted = await dbContext.ProductsForRead.AnyAsync();
        productWasPersisted.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativePrice_ReturnsValidationFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(UtcNow));

        CreateProductCommand command = new(
            "Keyboard",
            -0.01m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        Error error = ShouldBeValidationFailure(result);
        error.Message.ShouldBe("Price cannot be negative.");

        bool productWasPersisted = await dbContext.ProductsForRead.AnyAsync();
        productWasPersisted.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidCurrency_ReturnsValidationFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(UtcNow));

        CreateProductCommand command = new(
            "Keyboard",
            99.90m,
            "CH");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        Error error = ShouldBeValidationFailure(result);
        error.Message.ShouldStartWith("Currency must be an ISO 4217 three-letter code.");

        bool productWasPersisted = await dbContext.ProductsForRead.AnyAsync();
        productWasPersisted.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyDecimalPlaces_ReturnsValidationFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(UtcNow));

        CreateProductCommand command = new(
            "Keyboard",
            99.999m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        Error error = ShouldBeValidationFailure(result);
        error.Message.ShouldStartWith($"Price cannot have more than {Money.Scale} decimal places.");

        bool productWasPersisted = await dbContext.ProductsForRead.AnyAsync();
        productWasPersisted.ShouldBeFalse();
    }

    private static Error ShouldBeValidationFailure<T>(Result<T> result)
        where T : notnull
    {
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();

        Error error = result.Error.ShouldNotBeNull();
        error.Type.ShouldBe(ErrorType.Validation);
        error.Code.ShouldBe("validation_error");

        return error;
    }
}
