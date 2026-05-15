using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.GetProducts;
using Company.Template.Domain.Products;
using Company.Template.Infrastructure.Persistence;
using Company.Template.Infrastructure.Persistence.Queries;

namespace Company.Template.Application.Tests.Products.GetProducts;

public sealed class GetProductsUseCaseTests : IClassFixture<TestDatabase>, IAsyncLifetime
{
    private static readonly DateTimeOffset AlphaCreatedAt = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset BetaCreatedAt = new(2026, 1, 2, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DeltaCreatedAt = new(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DiscontinuedAt = new(2026, 1, 7, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DiscontinuedCreatedAt = new(2026, 1, 6, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EchoCreatedAt = new(2026, 1, 5, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset GammaCreatedAt = new(2026, 1, 3, 10, 0, 0, TimeSpan.Zero);

    private readonly TestDatabase _database;

    public GetProductsUseCaseTests(TestDatabase database)
    {
        _database = database;
    }

    public async Task InitializeAsync()
    {
        await using ApplicationDbContext dbContext = await _database.CreateCleanDbContextAsync();

        dbContext.Products.AddRange(
            CreateProduct("Alpha Keyboard", 99.90m, "CHF", AlphaCreatedAt),
            CreateProduct("Beta Mouse", 49.90m, "CHF", BetaCreatedAt),
            CreateProduct("Gamma Keyboard", 129.00m, "EUR", GammaCreatedAt),
            CreateProduct("Delta Monitor", 299.00m, "CHF", DeltaCreatedAt),
            CreateProduct("Echo Cable", 9.90m, "USD", EchoCreatedAt),
            CreateDiscontinuedProduct());

        await dbContext.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithoutFilters_ReturnsActiveProductsOnly()
    {
        // Arrange
        await using ApplicationDbContext dbContext = _database.CreateDbContext();
        GetProductsUseCase useCase = CreateUseCase(dbContext);

        GetProductsQuery query = new(
            CreatePage(1, 20),
            CreateFilter(),
            CreateSort("createdAt", "asc"));

        // Act
        Result<PagedResult<ProductDto>> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        PagedResult<ProductDto> page = result.Value;

        page.PageNumber.ShouldBe(1);
        page.PageSize.ShouldBe(20);
        page.TotalCount.ShouldBe(5);
        page.TotalPages.ShouldBe(1);
        page.HasNextPage.ShouldBeFalse();
        page.HasPreviousPage.ShouldBeFalse();

        page.Items.Select(product => product.Name)
            .ShouldBe(
             [
                 "Alpha Keyboard",
                 "Beta Mouse",
                 "Gamma Keyboard",
                 "Delta Monitor",
                 "Echo Cable"
             ]);

        page.Items.ShouldAllBe(product => product.Status == ProductStatus.Active);
    }

    [Fact]
    public async Task ExecuteAsync_WithSearchFilter_ReturnsMatchingActiveProducts()
    {
        // Arrange
        await using ApplicationDbContext dbContext = _database.CreateDbContext();
        GetProductsUseCase useCase = CreateUseCase(dbContext);

        GetProductsQuery query = new(
            CreatePage(1, 20),
            CreateFilter("Keyboard"),
            CreateSort("name", "asc"));

        // Act
        Result<PagedResult<ProductDto>> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        PagedResult<ProductDto> page = result.Value;

        page.TotalCount.ShouldBe(2);
        page.Items.Select(product => product.Name)
            .ShouldBe(
             [
                 "Alpha Keyboard",
                 "Gamma Keyboard"
             ]);
    }

    [Fact]
    public async Task ExecuteAsync_WithCurrencyFilter_ReturnsMatchingActiveProducts()
    {
        // Arrange
        await using ApplicationDbContext dbContext = _database.CreateDbContext();
        GetProductsUseCase useCase = CreateUseCase(dbContext);

        GetProductsQuery query = new(
            CreatePage(1, 20),
            CreateFilter(currency: "CHF"),
            CreateSort("name", "asc"));

        // Act
        Result<PagedResult<ProductDto>> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        PagedResult<ProductDto> page = result.Value;

        page.TotalCount.ShouldBe(3);
        page.Items.Select(product => product.Name)
            .ShouldBe(
             [
                 "Alpha Keyboard",
                 "Beta Mouse",
                 "Delta Monitor"
             ]);

        page.Items.ShouldAllBe(product => product.Price.Currency.Code == "CHF");
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveStatusFilter_ReturnsActiveProducts()
    {
        // Arrange
        await using ApplicationDbContext dbContext = _database.CreateDbContext();
        GetProductsUseCase useCase = CreateUseCase(dbContext);

        GetProductsQuery query = new(
            CreatePage(1, 20),
            CreateFilter(status: "active"),
            CreateSort("name", "asc"));

        // Act
        Result<PagedResult<ProductDto>> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        PagedResult<ProductDto> page = result.Value;

        page.TotalCount.ShouldBe(5);
        page.Items.ShouldAllBe(product => product.Status == ProductStatus.Active);
    }

    [Fact]
    public async Task ExecuteAsync_WithNameSorting_ReturnsProductsInNameOrder()
    {
        // Arrange
        await using ApplicationDbContext dbContext = _database.CreateDbContext();
        GetProductsUseCase useCase = CreateUseCase(dbContext);

        GetProductsQuery query = new(
            CreatePage(1, 20),
            CreateFilter(),
            CreateSort("name", "desc"));

        // Act
        Result<PagedResult<ProductDto>> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        result.Value.Items.Select(product => product.Name)
              .ShouldBe(
               [
                   "Gamma Keyboard",
                   "Echo Cable",
                   "Delta Monitor",
                   "Beta Mouse",
                   "Alpha Keyboard"
               ]);
    }

    [Fact]
    public async Task ExecuteAsync_WithPriceSorting_ReturnsProductsInPriceOrder()
    {
        // Arrange
        await using ApplicationDbContext dbContext = _database.CreateDbContext();
        GetProductsUseCase useCase = CreateUseCase(dbContext);

        GetProductsQuery query = new(
            CreatePage(1, 20),
            CreateFilter(),
            CreateSort("price", "asc"));

        // Act
        Result<PagedResult<ProductDto>> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        result.Value.Items.Select(product => product.Name)
              .ShouldBe(
               [
                   "Echo Cable",
                   "Beta Mouse",
                   "Alpha Keyboard",
                   "Gamma Keyboard",
                   "Delta Monitor"
               ]);
    }

    [Fact]
    public async Task ExecuteAsync_WithCreatedAtSorting_ReturnsProductsInCreatedAtOrder()
    {
        // Arrange
        await using ApplicationDbContext dbContext = _database.CreateDbContext();
        GetProductsUseCase useCase = CreateUseCase(dbContext);

        GetProductsQuery query = new(
            CreatePage(1, 20),
            CreateFilter(),
            CreateSort("createdAt", "desc"));

        // Act
        Result<PagedResult<ProductDto>> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        result.Value.Items.Select(product => product.Name)
              .ShouldBe(
               [
                   "Echo Cable",
                   "Delta Monitor",
                   "Gamma Keyboard",
                   "Beta Mouse",
                   "Alpha Keyboard"
               ]);
    }

    [Fact]
    public async Task ExecuteAsync_WithPaging_ReturnsRequestedPage()
    {
        // Arrange
        await using ApplicationDbContext dbContext = _database.CreateDbContext();
        GetProductsUseCase useCase = CreateUseCase(dbContext);

        GetProductsQuery query = new(
            CreatePage(2, 2),
            CreateFilter(),
            CreateSort("name", "asc"));

        // Act
        Result<PagedResult<ProductDto>> result = await useCase.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        PagedResult<ProductDto> page = result.Value;

        page.PageNumber.ShouldBe(2);
        page.PageSize.ShouldBe(2);
        page.TotalCount.ShouldBe(5);
        page.TotalPages.ShouldBe(3);
        page.HasNextPage.ShouldBeTrue();
        page.HasPreviousPage.ShouldBeTrue();

        page.Items.Select(product => product.Name)
            .ShouldBe(
             [
                 "Delta Monitor",
                 "Echo Cable"
             ]);
    }

    private static GetProductsUseCase CreateUseCase(ApplicationDbContext dbContext)
    {
        return new GetProductsUseCase(new ProductQueries(dbContext));
    }

    private static Product CreateProduct(
        string name,
        decimal price,
        string currency,
        DateTimeOffset createdAt)
    {
        return Product.Create(
            ProductName.Create(name),
            Money.Create(price, Currency.Create(currency)),
            createdAt);
    }

    private static Product CreateDiscontinuedProduct()
    {
        Product product = CreateProduct(
            "Zeta Keyboard",
            399.00m,
            "CHF",
            DiscontinuedCreatedAt);

        product.Discontinue(DiscontinuedAt);

        return product;
    }

    private static PageRequest CreatePage(
        int pageNumber,
        int pageSize)
    {
        Result<PageRequest> result = PageRequest.Create(pageNumber, pageSize);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }

    private static ProductFilter CreateFilter(
        string? search = null,
        string? status = null,
        string? currency = null)
    {
        Result<ProductFilter> result = ProductFilter.Create(search, status, currency);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }

    private static ProductSort CreateSort(
        string? sortBy = null,
        string? sortDirection = null)
    {
        Result<ProductSort> result = ProductSort.Create(sortBy, sortDirection);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }
}
