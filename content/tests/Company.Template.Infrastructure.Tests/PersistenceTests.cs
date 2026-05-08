using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;
using Company.Template.Domain.Products;
using Company.Template.Infrastructure.Persistence;
using Company.Template.Infrastructure.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Company.Template.Infrastructure.Tests;

public sealed class PersistenceTests : IClassFixture<TestDatabase>
{
    private readonly TestDatabase _database;

    public PersistenceTests(TestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task CanPersistAndLoadProduct()
    {
        await using var dbContext = new ApplicationDbContext(
            _database.CreateDbContextOptions(),
            new NoOpDomainEventDispatcher());

        await dbContext.Database.EnsureCreatedAsync();

        var product = Product.Create(
            ProductName.Create("Keyboard"),
            Money.Create(99.99m, "USD"),
            DateTimeOffset.UtcNow);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var loaded = await dbContext.Products.SingleAsync(entity => entity.Id == product.Id);

        loaded.Id.ShouldBe(product.Id);
        loaded.Name.Value.ShouldBe("Keyboard");
        loaded.Price.Amount.ShouldBe(99.99m);
        loaded.Price.Currency.ShouldBe("USD");
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
