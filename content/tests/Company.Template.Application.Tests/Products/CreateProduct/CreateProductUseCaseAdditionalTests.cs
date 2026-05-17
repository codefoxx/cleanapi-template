using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Domain.Products;
using Company.Template.Infrastructure.Persistence;
using Company.Template.TestSupport.Application;

namespace Company.Template.Application.Tests.Products.CreateProduct;

[Collection(DatabaseCollection.Name)]
public sealed class CreateProductUseCaseAdditionalTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly TestDatabaseServer _server;

    public CreateProductUseCaseAdditionalTests(TestDatabaseServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task ExecuteAsync_WithTooLongName_ReturnsValidationFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(UtcNow));

        CreateProductCommand command = new(
            new string('A', ProductName.MaxLength + 1),
            99.90m,
            "CHF");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        AssertError(result, ErrorType.Validation, ErrorCodes.ProductNameTooLong);
        await AssertProductWasNotPersistedAsync(dbContext);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnsupportedCurrency_ReturnsValidationFailure()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

        CreateProductUseCase useCase = new(
            dbContext,
            new FixedClock(UtcNow));

        CreateProductCommand command = new(
            "Keyboard",
            99.90m,
            "ABC");

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(command, CancellationToken.None);

        // Assert
        AssertError(result, ErrorType.Validation, ErrorCodes.CurrencyUnsupported);
        await AssertProductWasNotPersistedAsync(dbContext);
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

    private static async Task AssertProductWasNotPersistedAsync(ApplicationDbContext dbContext)
    {
        bool productWasPersisted = await dbContext.Products
                                                  .AsNoTracking()
                                                  .AnyAsync();

        productWasPersisted.ShouldBeFalse();
    }
}