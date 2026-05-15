using Company.Template.Application.Abstractions.DomainEvents;
using Company.Template.Domain.Common;
using Company.Template.Domain.Products;
using Company.Template.Infrastructure.Persistence;

namespace Company.Template.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextDomainEventTests : IClassFixture<TestDatabase>
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly TestDatabase _database;

    public ApplicationDbContextDomainEventTests(TestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task SaveChangesAsync_WithRecordedDomainEvents_DispatchesDomainEvents()
    {
        // Arrange
        RecordingDomainEventDispatcher dispatcher = new();

        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync(dispatcher);

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

        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync(dispatcher);

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

        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync(dispatcher);

        Product product = CreateProduct();

        dbContext.Products.Add(product);

        // Act
        InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>(
            dbContext.SaveChangesAsync());

        // Assert
        exception.Message.ShouldBe(ThrowingDomainEventDispatcher.ExceptionMessage);

        await using ApplicationDbContext verificationContext = _database.CreateDbContext();

        Product? persistedProduct = await verificationContext.Products.FindAsync(product.Id);

        persistedProduct.ShouldNotBeNull();
        persistedProduct.Name.ShouldBe(ProductName.Create("Mechanical Keyboard"));
    }

    [Fact]
    public async Task SaveChangesAsync_WhenDispatcherThrows_ClearsDomainEvents()
    {
        // Arrange
        ThrowingDomainEventDispatcher dispatcher = new();

        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync(dispatcher);

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

        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync(dispatcher);

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

    private sealed class RecordingDomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly List<IDomainEvent> _dispatchedEvents = [];

        public IReadOnlyList<IDomainEvent> DispatchedEvents => _dispatchedEvents;

        public Task DispatchAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents,
            CancellationToken cancellationToken)
        {
            _dispatchedEvents.AddRange(domainEvents);

            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingDomainEventDispatcher : IDomainEventDispatcher
    {
        public const string ExceptionMessage = "Domain event dispatch failed.";

        public Task DispatchAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(ExceptionMessage);
        }
    }
}
