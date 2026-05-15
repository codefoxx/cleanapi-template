using Company.Template.Application.Abstractions.DomainEvents;
using Company.Template.Domain.Common;
using Company.Template.Domain.Products;
using Company.Template.Infrastructure.Persistence;

namespace Company.Template.Infrastructure.Tests;

public sealed class PersistenceTests : IClassFixture<TestDatabase>
{
    private readonly TestDatabase _database;

    public PersistenceTests(TestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task SaveChanges_WhenProductIsAdded_ShouldPersistAndReloadProduct()
    {
        // Arrange
        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync();

        Product product = Product.Create(
            ProductName.Create("Keyboard"),
            Money.Create(99.99m, "USD"),
            DateTimeOffset.UtcNow);

        // Act
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        Product loaded = await dbContext.Products
                                        .AsNoTracking()
                                        .SingleAsync(entity => entity.Id == product.Id);

        // Assert
        loaded.Id.ShouldBe(product.Id);
        loaded.Name.Value.ShouldBe("Keyboard");
        loaded.Price.Amount.ShouldBe(99.99m);
        loaded.Price.Currency.Code.ShouldBe("USD");
    }
}
