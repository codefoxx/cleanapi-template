using Company.Template.Domain.Products;

namespace Company.Template.Domain.Tests.Products;

public sealed class ProductIdTests
{
    [Fact]
    public void New_ReturnsNonEmptyProductId()
    {
        // Act
        var productId = ProductId.New();

        // Assert
        productId.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void From_WithNonEmptyGuid_ReturnsProductId()
    {
        // Arrange
        var value = Guid.CreateVersion7();

        // Act
        var productId = ProductId.From(value);

        // Assert
        productId.Value.ShouldBe(value);
    }

    [Fact]
    public void From_WithEmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        Guid value = Guid.Empty;

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => ProductId.From(value));

        // Assert
        exception.ParamName.ShouldBe("value");
    }

    [Fact]
    public void ToString_ReturnsGuidValue()
    {
        // Arrange
        var value = Guid.CreateVersion7();
        var productId = ProductId.From(value);

        // Act
        string result = productId.ToString();

        // Assert
        result.ShouldBe(value.ToString());
    }
}
