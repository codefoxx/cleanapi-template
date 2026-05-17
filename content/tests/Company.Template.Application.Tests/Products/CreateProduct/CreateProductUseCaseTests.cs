using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Domain.Products;
using Company.Template.Domain.SharedKernel;
using Company.Template.Infrastructure.Persistence;
using Company.Template.TestSupport.Application;

namespace Company.Template.Application.Tests.Products.CreateProduct;

[Collection(DatabaseCollection.Name)]
public sealed class CreateProductUseCaseTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly TestDatabaseServer _server;

    public CreateProductUseCaseTests(TestDatabaseServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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
        result.Error.IsNone.ShouldBeTrue();

        ProductDto product = result.Value;

        product.Id.ShouldNotBe(Guid.Empty);
        product.Name.ShouldBe("Keyboard");
        product.Price.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
        product.Status.ShouldBe(ProductStatus.Active);
        product.CreatedAt.ShouldBe(UtcNow);
        product.DiscontinuedAt.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_PersistsProduct()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Id.Value.ShouldBe(product.Id);
        persistedProduct.Name.ShouldBe(ProductName.Create("Keyboard"));
        persistedProduct.Price.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
        persistedProduct.Status.ShouldBe(ProductStatus.Active);
        persistedProduct.CreatedAt.ShouldBe(UtcNow);
        persistedProduct.DiscontinuedAt.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_DispatchesProductCreatedDomainEvent()
    {
        // Arrange
        RecordingDomainEventDispatcher domainEventDispatcher = new();

        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext(domainEventDispatcher);

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
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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
        AssertError(result, ErrorType.Validation, ErrorCodes.ProductNameRequired);

        bool productWasPersisted = await dbContext.Products
                                                  .AsNoTracking()
                                                  .AnyAsync();
        productWasPersisted.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithTooLongName_ReturnsValidationFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(UtcNow));

        CreateProductCommand command = new(
            new string('A', ProductName.MaxLength + 1),
            99.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        AssertError(result, ErrorType.Validation, ErrorCodes.ProductNameTooLong);

        bool productWasPersisted = await dbContext.Products
                                                  .AsNoTracking()
                                                  .AnyAsync();
        productWasPersisted.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativePrice_ReturnsValidationFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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
        AssertError(result, ErrorType.Validation, ErrorCodes.AmountNegative);

        bool productWasPersisted = await dbContext.Products
                                                  .AsNoTracking()
                                                  .AnyAsync();
        productWasPersisted.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidCurrency_ReturnsValidationFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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
        AssertError(result, ErrorType.Validation, ErrorCodes.CurrencyInvalidFormat);

        bool productWasPersisted = await dbContext.Products
                                                  .AsNoTracking()
                                                  .AnyAsync();
        productWasPersisted.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithUnsupportedCurrency_ReturnsValidationFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(UtcNow));

        CreateProductCommand command = new(
            "Keyboard",
            99.90m,
            "ABC");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        AssertError(result, ErrorType.Validation, ErrorCodes.CurrencyUnsupported);

        bool productWasPersisted = await dbContext.Products
                                                  .AsNoTracking()
                                                  .AnyAsync();
        productWasPersisted.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyDecimalPlaces_ReturnsValidationFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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
        AssertError(result, ErrorType.Validation, ErrorCodes.AmountTooManyDecimalPlaces);

        bool productWasPersisted = await dbContext.Products
                                                  .AsNoTracking()
                                                  .AnyAsync();
        productWasPersisted.ShouldBeFalse();
    }

    private static Error AssertError<T>(
        Result<T> result,
        ErrorType expectedType,
        ErrorCode expectedCode)
        where T : notnull
    {
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();

        Error error = result.Error;

        error.ShouldNotBe(Error.None);
        error.Type.ShouldBe(expectedType);
        error.Code.ShouldBeEquivalentTo(expectedCode);

        return error;
    }
}
