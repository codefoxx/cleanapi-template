using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.GetProducts;
using Company.Template.Infrastructure.Persistence;
using Company.Template.Infrastructure.Persistence.Queries;
using Company.Template.TestSupport.Application;

namespace Company.Template.Application.Tests.Products.GetProducts;

[Collection(DatabaseCollection.Name)]
public sealed class GetProductsUseCaseAdditionalTests
{
    private readonly TestDatabaseServer _server;

    public GetProductsUseCaseAdditionalTests(TestDatabaseServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyCatalog_ReturnsEmptyPage()
    {
        // Arrange
        await using TestDatabase database = await TestDatabase.CreateAsync(_server);
        await using ApplicationDbContext dbContext = database.CreateDbContext();

        GetProductsUseCase useCase = new(new ProductQueries(dbContext));

        GetProductsQuery query = new(
            CreatePage(1, 20),
            ProductFilter.Empty,
            ProductSort.Default);

        // Act
        Result<PagedResult<ProductDto>> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        PagedResult<ProductDto> page = result.Value;

        page.PageNumber.ShouldBe(1);
        page.PageSize.ShouldBe(20);
        page.TotalCount.ShouldBe(0);
        page.TotalPages.ShouldBe(0);
        page.HasNextPage.ShouldBeFalse();
        page.HasPreviousPage.ShouldBeFalse();
        page.Items.ShouldBeEmpty();
    }

    private static PageRequest CreatePage(int pageNumber, int pageSize)
    {
        Result<PageRequest> result = PageRequest.Create(pageNumber, pageSize);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }
}