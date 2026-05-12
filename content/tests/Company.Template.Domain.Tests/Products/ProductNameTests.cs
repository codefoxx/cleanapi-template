using Company.Template.Domain.Products;

namespace Company.Template.Domain.Tests.Products;

public sealed class ProductNameTests
{
    [Fact]
    public void Create_WithValidName_ReturnsProductName()
    {
        // Arrange
        const string value = "Keyboard";

        // Act
        var productName = ProductName.Create(value);

        // Assert
        productName.Value.ShouldBe(value);
    }

    [Fact]
    public void Create_WithLeadingAndTrailingWhitespace_TrimsName()
    {
        // Arrange
        const string value = "  Keyboard  ";

        // Act
        var productName = ProductName.Create(value);

        // Assert
        productName.Value.ShouldBe("Keyboard");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Create_WithMissingName_ThrowsArgumentException(string value)
    {
        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => ProductName.Create(value));

        // Assert
        exception.Message.ShouldStartWith("Product name is required.");
        exception.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        string value = null!;

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => ProductName.Create(value));

        // Assert
        exception.Message.ShouldStartWith("Product name is required.");
        exception.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Create_WithNameAtMaximumLength_ReturnsProductName()
    {
        // Arrange
        string value = new('A', ProductName.MaxLength);

        // Act
        var productName = ProductName.Create(value);

        // Assert
        productName.Value.ShouldBe(value);
    }

    [Fact]
    public void Create_WithNameLongerThanMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        string value = new('A', ProductName.MaxLength + 1);

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => ProductName.Create(value));

        // Assert
        exception.Message.ShouldStartWith($"Product name cannot exceed {ProductName.MaxLength} characters.");
        exception.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var first = ProductName.Create("Keyboard");
        var second = ProductName.Create("Keyboard");

        // Act
        bool isEqual = first.Equals(second);

        // Assert
        isEqual.ShouldBeTrue();
        first.ShouldBe(second);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var productName = ProductName.Create("Keyboard");

        // Act
        string value = productName.ToString();

        // Assert
        value.ShouldBe("Keyboard");
    }
}
