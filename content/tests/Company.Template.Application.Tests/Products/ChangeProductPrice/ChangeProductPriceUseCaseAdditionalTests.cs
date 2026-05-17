using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.ChangeProductPrice;
using Company.Template.Domain.Products;
using Company.Template.Domain.SharedKernel;
using Company.Template.Infrastructure.Persistence;
using Company.Template.TestSupport.Application;

namespace Company.Template.Application.Tests.Products.ChangeProductPrice;

[Collection(DatabaseCollection.Name)]
public sealed class ChangeProductPriceUseCaseAdditionalTests
{
    private static readonly DateTimeOffset ChangedAt = new(2026, 1, 2, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly TestDatabaseServer _server;

    public ChangeProductPriceUseCaseAdditionalTests(TestDatabaseServer server)
    {
        _server = server;
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

    private static void AssertError<T>(
        Result<T> result,
        ErrorType expectedType,
        ErrorCode expectedCode)
        where T : notnull
    {
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.Type.ShouldBe(expectedType);
        result.Error.Code.ShouldBeEquivalentTo(expectedCode);
    }
}