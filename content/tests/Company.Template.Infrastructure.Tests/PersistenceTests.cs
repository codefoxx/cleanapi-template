using Company.Template.Domain.Products;
using Company.Template.Domain.SharedKernel;
using Company.Template.Infrastructure.Persistence;

namespace Company.Template.Infrastructure.Tests;

[Collection(DatabaseCollection.Name)]
public sealed class PersistenceTests
{
    private readonly TestDatabaseServer _server;

    public PersistenceTests(TestDatabaseServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task SaveChanges_WhenProductIsAdded_ShouldPersistAndReloadProduct()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

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
