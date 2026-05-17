using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.ChangeProductPrice;
using Company.Template.Domain.Common;
using Company.Template.Domain.Products;
using Company.Template.Domain.SharedKernel;
using Company.Template.Infrastructure.Persistence;
using Company.Template.TestSupport.Application;

namespace Company.Template.Application.Tests.Products.ChangeProductPrice;

[Collection(DatabaseCollection.Name)]
public sealed class ChangeProductPriceUseCaseTests
{
    private static readonly DateTimeOffset ChangedAt = new(2026, 1, 2, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DiscontinuedAt = new(2026, 1, 3, 10, 0, 0, TimeSpan.Zero);

    private readonly TestDatabaseServer _server;

    public ChangeProductPriceUseCaseTests(TestDatabaseServer server)
    {
        _server = server;
    }


    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();
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
        dto.Price.ShouldBe(Money.Create(129.90m, Iso4217CurrencyCodes.Chf));
        dto.Status.ShouldBe(ProductStatus.Active);
        dto.CreatedAt.ShouldBe(CreatedAt);
        dto.DiscontinuedAt.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_PersistsChangedPrice()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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

        persistedProduct.Price.ShouldBe(Money.Create(129.90m, Iso4217CurrencyCodes.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_DispatchesProductPriceChangedDomainEvent()
    {
        // Arrange
        RecordingDomainEventDispatcher domainEventDispatcher = new();

        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext(domainEventDispatcher);

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
        domainEvent.OldPrice.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
        domainEvent.NewPrice.ShouldBe(Money.Create(129.90m, Iso4217CurrencyCodes.Chf));
        domainEvent.OccurredAt.ShouldBe(ChangedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingProductId_ReturnsValidationFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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
        AssertError(result, ErrorType.Validation, ErrorCodes.ProductIdRequired);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProductId_ReturnsNotFoundFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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
        result.Error.Code.ShouldBeEquivalentTo(ErrorCodes.NotFound);
        result.Error.Message.ShouldBe("Product was not found.");
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativePrice_ReturnsValidationFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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
        AssertError(result, ErrorType.Validation, ErrorCodes.AmountNegative);

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidCurrency_ReturnsValidationFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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
        AssertError(result, ErrorType.Validation, ErrorCodes.CurrencyInvalidFormat);

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyDecimalPlaces_ReturnsValidationFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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
        AssertError(result, ErrorType.Validation, ErrorCodes.AmountTooManyDecimalPlaces);

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductIsDiscontinued_ReturnsConflictFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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
        AssertError(result, ErrorType.Conflict, ErrorCodes.DiscontinuedProductCannotBeChanged);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductIsDiscontinued_DoesNotChangePrice()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
        persistedProduct.Status.ShouldBe(ProductStatus.Discontinued);
        persistedProduct.DiscontinuedAt.ShouldBe(DiscontinuedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnsupportedCurrency_ReturnsValidationFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

        Product product = await PersistProductAsync(dbContext);

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            product.Id.Value,
            129.90m,
            "ABC");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        AssertError(result, ErrorType.Validation, ErrorCodes.CurrencyUnsupported);

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WithSamePrice_DoesNotDispatchProductPriceChangedDomainEvent()
    {
        // Arrange
        RecordingDomainEventDispatcher domainEventDispatcher = new();

        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext(domainEventDispatcher);

        Product product = await PersistProductAsync(dbContext);
        domainEventDispatcher.ClearDispatchedEvents();

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            product.Id.Value,
            99.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        domainEventDispatcher.DispatchedEvents.ShouldBeEmpty();
    }

    private static async Task<Product> PersistProductAsync(ApplicationDbContext dbContext)
    {
        Product product = Product.Create(
            ProductName.Create("Keyboard"),
            Money.Create(99.90m, Iso4217CurrencyCodes.Chf),
            CreatedAt);

        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync(CancellationToken.None);

        product.ClearDomainEvents();

        return product;
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
