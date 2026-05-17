using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Application.Products.DiscontinueProduct;
using Company.Template.Infrastructure.Persistence;
using Company.Template.TestSupport.Application;

namespace Company.Template.Application.Tests.Products.DiscontinueProduct;

[Collection(DatabaseCollection.Name)]
public sealed class DiscontinueProductUseCaseAdditionalTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DiscontinuedAt = new(2026, 1, 2, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset LaterDiscontinuedAt = new(2026, 1, 3, 10, 0, 0, TimeSpan.Zero);

    private readonly TestDatabaseServer _server;

    public DiscontinueProductUseCaseAdditionalTests(TestDatabaseServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyDiscontinuedProduct_DoesNotDispatchDomainEventAgain()
    {
        // Arrange
        RecordingDomainEventDispatcher domainEventDispatcher = new();

        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext(domainEventDispatcher);

        ProductDto product = await CreateProductAsync(dbContext);

        DiscontinueProductUseCase firstUseCase = new(
            dbContext,
            new FixedClock(DiscontinuedAt));

        Result firstResult = await firstUseCase.ExecuteAsync(
            new DiscontinueProductCommand(product.Id),
            CancellationToken.None);

        firstResult.IsSuccess.ShouldBeTrue();
        domainEventDispatcher.ClearDispatchedEvents();

        DiscontinueProductUseCase secondUseCase = new(
            dbContext,
            new FixedClock(LaterDiscontinuedAt));

        // Act
        Result result = await secondUseCase.ExecuteAsync(
            new DiscontinueProductCommand(product.Id),
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        domainEventDispatcher.DispatchedEvents.ShouldBeEmpty();
    }

    private static async Task<ProductDto> CreateProductAsync(ApplicationDbContext dbContext)
    {
        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(CreatedAt));

        CreateProductCommand command = new(
            "Keyboard",
            99.90m,
            "CHF");

        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }
}