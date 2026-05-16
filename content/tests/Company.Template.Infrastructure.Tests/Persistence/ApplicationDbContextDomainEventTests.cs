using Company.Template.Domain.Products;
using Company.Template.Domain.SharedKernel;
using Company.Template.Infrastructure.Persistence;
using Company.Template.TestSupport.Application.DomainEvents;

namespace Company.Template.Infrastructure.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class ApplicationDbContextDomainEventTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly TestDatabaseServer _server;

    public ApplicationDbContextDomainEventTests(TestDatabaseServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task SaveChangesAsync_WithRecordedDomainEvents_DispatchesDomainEvents()
    {
        // Arrange
        RecordingDomainEventDispatcher dispatcher = new();

        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext(dispatcher);

        Product product = CreateProduct();

        dbContext.Products.Add(product);

        // Act
        await dbContext.SaveChangesAsync();

        // Assert
        dispatcher.DispatchedEvents
                  .OfType<ProductCreatedDomainEvent>()
                  .ShouldContain(created => created.ProductId == product.Id);
    }

    [Fact]
    public async Task SaveChangesAsync_WithRecordedDomainEvents_ClearsDomainEventsAfterDispatch()
    {
        // Arrange
        RecordingDomainEventDispatcher dispatcher = new();

        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext(dispatcher);

        Product product = CreateProduct();

        dbContext.Products.Add(product);

        // Act
        await dbContext.SaveChangesAsync();

        // Assert
        product.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenDispatcherThrows_StillPersistsChanges()
    {
        // Arrange
        ThrowingDomainEventDispatcher dispatcher = new();

        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext(dispatcher);

        Product product = CreateProduct();

        dbContext.Products.Add(product);

        // Act
        InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>(
            dbContext.SaveChangesAsync());

        // Assert
        exception.Message.ShouldBe(ThrowingDomainEventDispatcher.ExceptionMessage);

        await using ApplicationDbContext verificationContext = database.CreateDbContext();

        Product? persistedProduct = await verificationContext.Products.FindAsync(product.Id);

        persistedProduct.ShouldNotBeNull();
        persistedProduct.Name.ShouldBe(ProductName.Create("Mechanical Keyboard"));
    }

    [Fact]
    public async Task SaveChangesAsync_WhenDispatcherThrows_ClearsDomainEvents()
    {
        // Arrange
        ThrowingDomainEventDispatcher dispatcher = new();

        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext(dispatcher);

        Product product = CreateProduct();

        dbContext.Products.Add(product);

        // Act
        await Should.ThrowAsync<InvalidOperationException>(
            dbContext.SaveChangesAsync());

        // Assert
        product.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WithoutRecordedDomainEvents_DoesNotDispatch()
    {
        // Arrange
        RecordingDomainEventDispatcher dispatcher = new();

        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext(dispatcher);

        // Act
        await dbContext.SaveChangesAsync();

        // Assert
        dispatcher.DispatchedEvents.ShouldBeEmpty();
    }

    private static Product CreateProduct()
    {
        return Product.Create(
            ProductName.Create("Mechanical Keyboard"),
            Money.Create(199.90m, Currency.Create("CHF")),
            CreatedAt);
    }
}
