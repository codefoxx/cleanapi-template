using Company.Template.Domain.Products;

namespace Company.Template.Domain.Tests.Products;

public sealed class CurrencyTests
{
    [Fact]
    public void Create_WithValidCode_ReturnsCurrency()
    {
        // Arrange
        const string code = "CHF";

        // Act
        var currency = Currency.Create(code);

        // Assert
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("CHF");
        currency.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithLowercaseCode_NormalizesCodeToUppercase()
    {
        // Arrange
        const string code = "chf";

        // Act
        var currency = Currency.Create(code);

        // Assert
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("CHF");
    }

    [Fact]
    public void Create_WithLeadingAndTrailingWhitespace_TrimsCode()
    {
        // Arrange
        const string code = "  chf  ";

        // Act
        var currency = Currency.Create(code);

        // Assert
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("CHF");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Create_WithMissingCode_ThrowsArgumentException(string code)
    {
        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Currency.Create(code));

        // Assert
        exception.Message.ShouldStartWith("Currency is required.");
        exception.ParamName.ShouldBe("code");
    }

    [Fact]
    public void Create_WithNullCode_ThrowsArgumentException()
    {
        // Arrange
        string code = null!;

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Currency.Create(code));

        // Assert
        exception.Message.ShouldStartWith("Currency is required.");
        exception.ParamName.ShouldBe("code");
    }

    [Theory]
    [InlineData("CH")]
    [InlineData("CHFF")]
    public void Create_WithCodeLengthOtherThanThree_ThrowsArgumentException(string code)
    {
        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Currency.Create(code));

        // Assert
        exception.Message.ShouldStartWith("Currency must be an ISO 4217 three-letter code.");
        exception.ParamName.ShouldBe("code");
    }

    [Fact]
    public void Create_WithValidCodeAndSymbol_ReturnsCurrency()
    {
        // Arrange
        const string code = "CHF";
        const string symbol = "Fr.";

        // Act
        var currency = Currency.Create(code, symbol);

        // Assert
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("Fr.");
        currency.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithLowercaseCodeAndSymbol_NormalizesCodeOnly()
    {
        // Arrange
        const string code = "chf";
        const string symbol = "Fr.";

        // Act
        var currency = Currency.Create(code, symbol);

        // Assert
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("Fr.");
    }

    [Fact]
    public void Create_WithLeadingAndTrailingWhitespaceAroundSymbol_TrimsSymbol()
    {
        // Arrange
        const string code = "CHF";
        const string symbol = "  Fr.  ";

        // Act
        var currency = Currency.Create(code, symbol);

        // Assert
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("Fr.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Create_WithMissingSymbol_ThrowsArgumentException(string symbol)
    {
        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Currency.Create("CHF", symbol));

        // Assert
        exception.Message.ShouldStartWith("Currency symbol is required.");
        exception.ParamName.ShouldBe("symbol");
    }

    [Fact]
    public void Create_WithNullSymbol_ThrowsArgumentException()
    {
        // Arrange
        string symbol = null!;

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Currency.Create("CHF", symbol));

        // Assert
        exception.Message.ShouldStartWith("Currency symbol is required.");
        exception.ParamName.ShouldBe("symbol");
    }

    [Theory]
    [InlineData("CH")]
    [InlineData("CHFF")]
    public void Create_WithSymbolAndCodeLengthOtherThanThree_ThrowsArgumentException(string code)
    {
        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Currency.Create(code, "Fr."));

        // Assert
        exception.Message.ShouldStartWith("Currency must be an ISO 4217 three-letter code.");
        exception.ParamName.ShouldBe("code");
    }

    [Fact]
    public void Empty_ReturnsEmptyCurrency()
    {
        // Act
        Currency currency = Currency.Empty;

        // Assert
        currency.Code.ShouldBeEmpty();
        currency.Symbol.ShouldBeEmpty();
        currency.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithSameCodeAndSymbol_ReturnsTrue()
    {
        // Arrange
        var first = Currency.Create("chf");
        var second = Currency.Create("CHF");

        // Act
        bool isEqual = first.Equals(second);

        // Assert
        isEqual.ShouldBeTrue();
        first.ShouldBe(second);
    }

    [Fact]
    public void Equals_WithSameCodeButDifferentSymbol_ReturnsFalse()
    {
        // Arrange
        var first = Currency.Create("CHF", "CHF");
        var second = Currency.Create("CHF", "Fr.");

        // Act
        bool isEqual = first.Equals(second);

        // Assert
        isEqual.ShouldBeFalse();
        first.ShouldNotBe(second);
    }

    [Fact]
    public void ToString_ReturnsCode()
    {
        // Arrange
        var currency = Currency.Create("CHF");

        // Act
        string value = currency.ToString();

        // Assert
        value.ShouldBe("CHF");
    }
}
