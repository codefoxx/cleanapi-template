using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.ChangeProductPrice;
using Company.Template.Domain.Common;
using Company.Template.Domain.Products;
using Company.Template.Infrastructure.Persistence;

namespace Company.Template.Application.Tests.Products.ChangeProductPrice;

public sealed class ChangeProductPriceUseCaseTests : IClassFixture<TestDatabase>
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ChangedAt = new(2026, 1, 2, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DiscontinuedAt = new(2026, 1, 3, 10, 0, 0, TimeSpan.Zero);

    private readonly TestDatabase _database;

    public ChangeProductPriceUseCaseTests(TestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        Product product = await PersistProductAsync(dbContext);

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            product.Id.Value,
            129.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.IsNone.ShouldBeTrue();

        ProductDto dto = result.Value;

        dto.Id.ShouldBe(product.Id.Value);
        dto.Name.ShouldBe("Keyboard");
        dto.Price.ShouldBe(Money.Create(129.90m, KnownCurrencies.Chf));
        dto.Status.ShouldBe(ProductStatus.Active);
        dto.CreatedAt.ShouldBe(CreatedAt);
        dto.DiscontinuedAt.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_PersistsChangedPrice()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        Product product = await PersistProductAsync(dbContext);

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            product.Id.Value,
            129.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(129.90m, KnownCurrencies.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_DispatchesProductPriceChangedDomainEvent()
    {
        // Arrange
        RecordingDomainEventDispatcher domainEventDispatcher = new();

        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync(domainEventDispatcher);

        Product product = await PersistProductAsync(dbContext);

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            product.Id.Value,
            129.90m,
            "CHF");

        domainEventDispatcher.ClearDispatchedEvents();

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        ProductPriceChangedDomainEvent domainEvent = domainEventDispatcher.DispatchedEvents
                                                                          .Single()
                                                                          .ShouldBeOfType<ProductPriceChangedDomainEvent>();

        domainEvent.ProductId.ShouldBe(product.Id);
        domainEvent.OldPrice.ShouldBe(Money.Create(99.90m, KnownCurrencies.Chf));
        domainEvent.NewPrice.ShouldBe(Money.Create(129.90m, KnownCurrencies.Chf));
        domainEvent.OccurredAt.ShouldBe(ChangedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingProductId_ReturnsValidationFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            Guid.Empty,
            129.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        AssertError(result, ErrorType.Validation, DomainErrorCodes.ProductIdRequired);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProductId_ReturnsNotFoundFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            Guid.CreateVersion7(),
            129.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Type.ShouldBe(ErrorType.NotFound);
        result.Error.Code.ShouldBe("not_found");
        result.Error.Message.ShouldBe("Product was not found.");
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativePrice_ReturnsValidationFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        Product product = await PersistProductAsync(dbContext);

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            product.Id.Value,
            -0.01m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        AssertError(result, ErrorType.Validation, DomainErrorCodes.AmountNegative);

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, KnownCurrencies.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidCurrency_ReturnsValidationFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        Product product = await PersistProductAsync(dbContext);

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            product.Id.Value,
            129.90m,
            "CH");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        AssertError(result, ErrorType.Validation, DomainErrorCodes.CurrencyInvalidFormat);

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, KnownCurrencies.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyDecimalPlaces_ReturnsValidationFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        Product product = await PersistProductAsync(dbContext);

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            product.Id.Value,
            129.999m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        AssertError(result, ErrorType.Validation, DomainErrorCodes.AmountTooManyDecimalPlaces);

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, KnownCurrencies.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductIsDiscontinued_ReturnsConflictFailure()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        Product product = await PersistProductAsync(dbContext);

        product.Discontinue(DiscontinuedAt);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            product.Id.Value,
            129.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        AssertError(result, ErrorType.Conflict, DomainErrorCodes.DiscontinuedProductCannotBeChanged);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductIsDiscontinued_DoesNotChangePrice()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateDbContextAsync();

        Product product = await PersistProductAsync(dbContext);

        product.Discontinue(DiscontinuedAt);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            product.Id.Value,
            129.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, KnownCurrencies.Chf));
        persistedProduct.Status.ShouldBe(ProductStatus.Discontinued);
        persistedProduct.DiscontinuedAt.ShouldBe(DiscontinuedAt);
    }

    private static async Task<Product> PersistProductAsync(ApplicationDbContext dbContext)
    {
        Product product = Product.Create(
            ProductName.Create("Keyboard"),
            Money.Create(99.90m, KnownCurrencies.Chf),
            CreatedAt);

        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync(CancellationToken.None);

        product.ClearDomainEvents();

        return product;
    }

    private static Error AssertError<T>(
        Result<T> result,
        ErrorType expectedType,
        DomainErrorCode expectedCode)
        where T : notnull
    {
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();

        Error error = result.Error;

        error.ShouldNotBe(Error.None);
        error.Type.ShouldBe(expectedType);
        error.Code.ShouldBe(expectedCode.Value);

        return error;
    }
}
