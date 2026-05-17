using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.GetProductById;
using Company.Template.Infrastructure.Persistence;
using Company.Template.Infrastructure.Persistence.Queries;
using Company.Template.TestSupport.Application;

namespace Company.Template.Application.Tests.Products.GetProductById;

[Collection(DatabaseCollection.Name)]
public sealed class GetProductByIdUseCaseAdditionalTests
{
    private readonly TestDatabaseServer _server;

    public GetProductByIdUseCaseAdditionalTests(TestDatabaseServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProductId_ReturnsNotFoundCode()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

        GetProductByIdUseCase useCase = new(new ProductQueries(dbContext));
        GetProductByIdQuery query = new(Guid.NewGuid());

        // Act
        Result<ProductDto> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Type.ShouldBe(ErrorType.NotFound);
        result.Error.Code.ShouldBeEquivalentTo(ErrorCodes.NotFound);
    }
}