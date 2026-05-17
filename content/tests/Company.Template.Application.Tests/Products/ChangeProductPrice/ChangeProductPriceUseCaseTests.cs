using Company.Template.Application.Products.ChangeProductPrice;
using Company.Template.Domain.Products;
using Company.Template.Domain.SharedKernel;

namespace Company.Template.Application.Tests.Products.ChangeProductPrice;

public sealed class ChangeProductPriceUseCaseTests : IClassFixture<ApplicationTestServer>
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ChangedAt = new(2026, 1, 2, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DiscontinuedAt = new(2026, 1, 3, 10, 0, 0, TimeSpan.Zero);

    private readonly ApplicationTestServer _server;

    public ChangeProductPriceUseCaseTests(ApplicationTestServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ChangesPriceAndReturnsUpdatedProduct()
    {
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

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(product.Id.Value);
        result.Value.Name.ShouldBe("Keyboard");
        result.Value.Price.ShouldBe(129.90m);
        result.Value.Currency.ShouldBe("CHF");
        result.Value.Status.ShouldBe(ProductStatus.Active);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_PersistsChangedPrice()
    {
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

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(129.90m, Iso4217CurrencyCodes.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_RecordsPriceChangedDomainEvent()
    {
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

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        Product persistedProduct = await dbContext.Products.SingleAsync();

        ProductPriceChangedDomainEvent domainEvent = persistedProduct.DomainEvents
                                                                      .ShouldHaveSingleItem()
                                                                      .ShouldBeOfType<ProductPriceChangedDomainEvent>();

        domainEvent.ProductId.ShouldBe(product.Id);
        domainEvent.OldPrice.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
        domainEvent.NewPrice.ShouldBe(Money.Create(129.90m, Iso4217CurrencyCodes.Chf));
        domainEvent.ChangedAt.ShouldBe(ChangedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithSamePrice_DoesNotRecordDomainEvent()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

        Product product = await PersistProductAsync(dbContext);

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            product.Id.Value,
            99.90m,
            "CHF");

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        Product persistedProduct = await dbContext.Products.SingleAsync();
        persistedProduct.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingProductId_ReturnsValidationFailure()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            Guid.Empty,
            129.90m,
            "CHF");

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        AssertError(result, ErrorType.Validation, DomainErrorCodes.ProductIdRequired);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProductId_ReturnsNotFoundFailure()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

        ChangeProductPriceUseCase useCase = new(
            dbContext,
            new FixedClock(ChangedAt));

        ChangeProductPriceCommand command = new(
            Guid.CreateVersion7(),
            129.90m,
            "CHF");

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Type.ShouldBe(ErrorType.NotFound);
        result.Error.Code.Value.ShouldBe(ErrorCodes.NotFound.Value);
        result.Error.Message.ShouldBe("Product was not found.");
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativePrice_ReturnsValidationFailure()
    {
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

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        AssertError(result, ErrorType.Validation, DomainErrorCodes.AmountNegative);

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidCurrency_ReturnsValidationFailure()
    {
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

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        AssertError(result, ErrorType.Validation, DomainErrorCodes.CurrencyInvalidFormat);

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyDecimalPlaces_ReturnsValidationFailure()
    {
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

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        AssertError(result, ErrorType.Validation, DomainErrorCodes.AmountTooManyDecimalPlaces);

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductIsDiscontinued_ReturnsConflictFailure()
    {
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

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        AssertError(result, ErrorType.Conflict, DomainErrorCodes.DiscontinuedProductCannotBeChanged);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductIsDiscontinued_DoesNotChangePrice()
    {
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

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();

        Product persistedProduct = await dbContext.Products
                                                  .AsNoTracking()
                                                  .SingleAsync();

        persistedProduct.Price.ShouldBe(Money.Create(99.90m, Iso4217CurrencyCodes.Chf));
        persistedProduct.Status.ShouldBe(ProductStatus.Discontinued);
        persistedProduct.DiscontinuedAt.ShouldBe(DiscontinuedAt);
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
        DomainErrorCode expectedCode)
        where T : notnull
    {
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();

        Error error = result.Error;

        error.ShouldNotBe(Error.None);
        error.Type.ShouldBe(expectedType);
        error.Code.Value.ShouldBe(expectedCode.Value);

        return error;
    }
}