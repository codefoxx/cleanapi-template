using Company.Template.Domain.Products;

namespace Company.Template.Domain.Tests.Products;

public sealed class ProductTests
{
    private static readonly DateTimeOffset ChangedAt = new(2026, 1, 2, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DiscontinuedAt = new(2026, 1, 3, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidValues_CreatesActiveProduct()
    {
        // Arrange
        ProductName name = ProductName.Create("Keyboard");
        Money price = Money.Create(99.90m, KnownCurrencies.Chf);

        // Act
        Product product = Product.Create(name, price, CreatedAt);

        // Assert
        product.Id.Value.ShouldNotBe(Guid.Empty);
        product.Name.ShouldBe(name);
        product.Price.ShouldBe(price);
        product.Status.ShouldBe(ProductStatus.Active);
        product.CreatedAt.ShouldBe(CreatedAt);
        product.DiscontinuedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_WithValidValues_RecordsProductCreatedDomainEvent()
    {
        // Arrange
        ProductName name = ProductName.Create("Keyboard");
        Money price = Money.Create(99.90m, KnownCurrencies.Chf);

        // Act
        Product product = Product.Create(name, price, CreatedAt);

        // Assert
        product.DomainEvents.Count.ShouldBe(1);

        ProductCreatedDomainEvent domainEvent = product.DomainEvents
                                                       .Single()
                                                       .ShouldBeOfType<ProductCreatedDomainEvent>();

        domainEvent.ProductId.ShouldBe(product.Id);
        domainEvent.OccurredAt.ShouldBe(CreatedAt);
    }

    [Fact]
    public void Create_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        ProductName name = null!;
        Money price = Money.Create(99.90m, KnownCurrencies.Chf);

        // Act
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => Product.Create(name, price, CreatedAt));

        // Assert
        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void Create_WithNullPrice_ThrowsArgumentNullException()
    {
        // Arrange
        ProductName name = ProductName.Create("Keyboard");
        Money price = null!;

        // Act
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => Product.Create(name, price, CreatedAt));

        // Assert
        exception.ParamName.ShouldBe("price");
    }

    [Fact]
    public void Rename_WithDifferentName_ChangesName()
    {
        // Arrange
        Product product = CreateProduct();
        ProductName newName = ProductName.Create("Mouse");

        // Act
        product.Rename(newName);

        // Assert
        product.Name.ShouldBe(newName);
    }

    [Fact]
    public void Rename_WithSameName_DoesNothing()
    {
        // Arrange
        Product product = CreateProduct();
        ProductName originalName = product.Name;

        // Act
        product.Rename(originalName);

        // Assert
        product.Name.ShouldBe(originalName);
    }

    [Fact]
    public void Rename_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        Product product = CreateProduct();
        ProductName newName = null!;

        // Act
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => product.Rename(newName));

        // Assert
        exception.ParamName.ShouldBe("newName");
    }

    [Fact]
    public void Rename_WhenProductIsDiscontinued_ThrowsInvalidOperationException()
    {
        // Arrange
        Product product = CreateProduct();
        product.Discontinue(DiscontinuedAt);

        ProductName newName = ProductName.Create("Mouse");

        // Act
        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => product.Rename(newName));

        // Assert
        exception.Message.ShouldBe("A discontinued product cannot be renamed.");
    }

    [Fact]
    public void ChangePrice_WithDifferentPrice_ChangesPrice()
    {
        // Arrange
        Product product = CreateProduct();
        Money newPrice = Money.Create(129.90m, KnownCurrencies.Chf);

        // Act
        product.ChangePrice(newPrice, ChangedAt);

        // Assert
        product.Price.ShouldBe(newPrice);
    }

    [Fact]
    public void ChangePrice_WithDifferentPrice_RecordsProductPriceChangedDomainEvent()
    {
        // Arrange
        Product product = CreateProduct();
        product.ClearDomainEvents();

        Money oldPrice = product.Price;
        Money newPrice = Money.Create(129.90m, KnownCurrencies.Chf);

        // Act
        product.ChangePrice(newPrice, ChangedAt);

        // Assert
        product.DomainEvents.Count.ShouldBe(1);

        ProductPriceChangedDomainEvent domainEvent = product.DomainEvents
                                                            .Single()
                                                            .ShouldBeOfType<ProductPriceChangedDomainEvent>();

        domainEvent.ProductId.ShouldBe(product.Id);
        domainEvent.OldPrice.ShouldBe(oldPrice);
        domainEvent.NewPrice.ShouldBe(newPrice);
        domainEvent.OccurredAt.ShouldBe(ChangedAt);
    }

    [Fact]
    public void ChangePrice_WithSamePrice_DoesNothing()
    {
        // Arrange
        Product product = CreateProduct();
        Money originalPrice = product.Price;

        product.ClearDomainEvents();

        // Act
        product.ChangePrice(originalPrice, ChangedAt);

        // Assert
        product.Price.ShouldBe(originalPrice);
        product.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void ChangePrice_WithNullPrice_ThrowsArgumentNullException()
    {
        // Arrange
        Product product = CreateProduct();
        Money newPrice = null!;

        // Act
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => product.ChangePrice(newPrice, ChangedAt));

        // Assert
        exception.ParamName.ShouldBe("newPrice");
    }

    [Fact]
    public void ChangePrice_WhenProductIsDiscontinued_CurrentlyAllowsPriceChange()
    {
        // Arrange
        Product product = CreateProduct();
        product.Discontinue(DiscontinuedAt);
        product.ClearDomainEvents();

        Money newPrice = Money.Create(129.90m, KnownCurrencies.Chf);

        // Act
        product.ChangePrice(newPrice, ChangedAt);

        // Assert
        product.Price.ShouldBe(newPrice);
        product.DomainEvents.Count.ShouldBe(1);
        product.DomainEvents.Single().ShouldBeOfType<ProductPriceChangedDomainEvent>();
    }

    [Fact]
    public void Discontinue_WhenProductIsActive_MarksProductAsDiscontinued()
    {
        // Arrange
        Product product = CreateProduct();

        // Act
        product.Discontinue(DiscontinuedAt);

        // Assert
        product.Status.ShouldBe(ProductStatus.Discontinued);
        product.DiscontinuedAt.ShouldBe(DiscontinuedAt);
    }

    [Fact]
    public void Discontinue_WhenProductIsActive_RecordsProductDiscontinuedDomainEvent()
    {
        // Arrange
        Product product = CreateProduct();
        product.ClearDomainEvents();

        // Act
        product.Discontinue(DiscontinuedAt);

        // Assert
        product.DomainEvents.Count.ShouldBe(1);

        ProductDiscontinuedDomainEvent domainEvent = product.DomainEvents
                                                            .Single()
                                                            .ShouldBeOfType<ProductDiscontinuedDomainEvent>();

        domainEvent.ProductId.ShouldBe(product.Id);
        domainEvent.OccurredAt.ShouldBe(DiscontinuedAt);
    }

    [Fact]
    public void Discontinue_WhenProductIsAlreadyDiscontinued_DoesNothing()
    {
        // Arrange
        Product product = CreateProduct();
        product.Discontinue(DiscontinuedAt);
        product.ClearDomainEvents();

        DateTimeOffset secondDiscontinuedAt = DiscontinuedAt.AddDays(1);

        // Act
        product.Discontinue(secondDiscontinuedAt);

        // Assert
        product.Status.ShouldBe(ProductStatus.Discontinued);
        product.DiscontinuedAt.ShouldBe(DiscontinuedAt);
        product.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_RemovesRecordedEvents()
    {
        // Arrange
        Product product = CreateProduct();
        product.DomainEvents.ShouldNotBeEmpty();

        // Act
        product.ClearDomainEvents();

        // Assert
        product.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void TryCreate_WithValidValues_ReturnsTrueAndProduct()
    {
        // Act
        bool result = Product.TryCreate(
            "Keyboard",
            99.90m,
            "CHF",
            CreatedAt,
            out Product? product,
            out DomainError? error);

        // Assert
        result.ShouldBeTrue();
        product.ShouldNotBeNull();
        product.Name.ShouldBe(ProductName.Create("Keyboard"));
        product.Price.ShouldBe(Money.Create(99.90m, KnownCurrencies.Chf));
        product.Status.ShouldBe(ProductStatus.Active);
        product.CreatedAt.ShouldBe(CreatedAt);
        error.ShouldBeNull();
    }

    [Fact]
    public void TryCreate_WithMissingName_ReturnsFalseAndDomainError()
    {
        // Act
        bool result = Product.TryCreate(
            " ",
            99.90m,
            "CHF",
            CreatedAt,
            out Product? product,
            out DomainError? error);

        // Assert
        result.ShouldBeFalse();
        product.ShouldBeNull();
        error.ShouldNotBeNull();
        error.Code.ShouldBe(DomainErrorCodes.ProductNameRequired);
        error.Message.ShouldBe("Product name is required.");
    }

    [Fact]
    public void TryCreate_WithInvalidPrice_ReturnsFalseAndDomainError()
    {
        // Act
        bool result = Product.TryCreate(
            "Keyboard",
            -0.01m,
            "CHF",
            CreatedAt,
            out Product? product,
            out DomainError? error);

        // Assert
        result.ShouldBeFalse();
        product.ShouldBeNull();
        error.ShouldNotBeNull();
        error.Code.ShouldBe(DomainErrorCodes.AmountNegative);
        error.Message.ShouldBe("Amount cannot be negative.");
    }

    private static Product CreateProduct()
    {
        return Product.Create(
            ProductName.Create("Keyboard"),
            Money.Create(99.90m, KnownCurrencies.Chf),
            CreatedAt);
    }
}
