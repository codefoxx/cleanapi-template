using Company.Template.Domain.SharedKernel;

namespace Company.Template.Domain.Tests.SharedKernel;

public sealed class CurrencyTests
{
    [Fact]
    public void Create_WithValidCode_ReturnsCurrency()
    {
        // Arrange
        const string code = "CHF";

        // Act
        Currency currency = Currency.Create(code);

        // Assert
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("CHF");
    }

    [Fact]
    public void Create_WithLowercaseCode_NormalizesCodeToUppercase()
    {
        // Act
        Currency currency = Currency.Create("chf");

        // Assert
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("CHF");
    }

    [Fact]
    public void Create_WithLeadingAndTrailingWhitespace_TrimsCode()
    {
        // Act
        Currency currency = Currency.Create("  chf  ");

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
        exception.ParamName.ShouldBe("code");
    }

    [Theory]
    [InlineData("CH")]
    [InlineData("CHFF")]
    [InlineData("12!")]
    public void Create_WithInvalidCodeFormat_ThrowsArgumentException(string code)
    {
        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Currency.Create(code));

        // Assert
        exception.ParamName.ShouldBe("code");
    }

    [Fact]
    public void Create_WithUnsupportedCode_ThrowsArgumentException()
    {
        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Currency.Create("ABC"));

        // Assert
        exception.ParamName.ShouldBe("code");
    }

    [Fact]
    public void Create_WithValidCodeAndSymbol_ReturnsCurrency()
    {
        // Act
        Currency currency = Currency.Create("CHF", "Fr.");

        // Assert
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("Fr.");
    }

    [Fact]
    public void Create_WithLowercaseCodeAndSymbol_NormalizesCodeOnly()
    {
        // Act
        Currency currency = Currency.Create("chf", "Fr.");

        // Assert
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("Fr.");
    }

    [Fact]
    public void Create_WithLeadingAndTrailingWhitespaceAroundSymbol_TrimsSymbol()
    {
        // Act
        Currency currency = Currency.Create("CHF", "  Fr.  ");

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
        exception.ParamName.ShouldBe("code");
    }

    [Fact]
    public void Create_WithNullSymbol_ThrowsArgumentException()
    {
        // Arrange
        string symbol = null!;

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Currency.Create("CHF", symbol));

        // Assert
        exception.ParamName.ShouldBe("code");
    }

    [Theory]
    [InlineData("CH")]
    [InlineData("CHFF")]
    [InlineData("12!")]
    public void Create_WithSymbolAndInvalidCodeFormat_ThrowsArgumentException(string code)
    {
        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Currency.Create(code, "Fr."));

        // Assert
        exception.ParamName.ShouldBe("code");
    }

    [Fact]
    public void Create_WithSymbolAndUnsupportedCode_ThrowsArgumentException()
    {
        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Currency.Create("ABC", "ABC"));

        // Assert
        exception.ParamName.ShouldBe("code");
    }

    [Fact]
    public void Equals_WithSameCodeAndSymbol_ReturnsTrue()
    {
        // Arrange
        Currency first = Currency.Create("chf");
        Currency second = Currency.Create("CHF");

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
        Currency first = Currency.Create("CHF", "CHF");
        Currency second = Currency.Create("CHF", "Fr.");

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
        Currency currency = Currency.Create("CHF");

        // Act
        string value = currency.ToString();

        // Assert
        value.ShouldBe("CHF");
    }

    [Fact]
    public void TryCreate_WithValidCode_ReturnsTrueAndCurrency()
    {
        // Act
        bool result = Currency.TryCreate(
            " chf ",
            out Currency? currency,
            out DomainError? error);

        // Assert
        result.ShouldBeTrue();
        currency.ShouldNotBeNull();
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("CHF");
        error.ShouldBeNull();
    }

    [Fact]
    public void TryCreate_WithValidCodeAndSymbol_ReturnsTrueAndCurrency()
    {
        // Act
        bool result = Currency.TryCreate(
            " usd ",
            " $ ",
            out Currency? currency,
            out DomainError? error);

        // Assert
        result.ShouldBeTrue();
        currency.ShouldNotBeNull();
        currency.Code.ShouldBe("USD");
        currency.Symbol.ShouldBe("$");
        error.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void TryCreate_WithMissingCode_ReturnsCurrencyRequired(string? code)
    {
        // Act
        bool result = Currency.TryCreate(
            code,
            out Currency? currency,
            out DomainError? error);

        // Assert
        result.ShouldBeFalse();
        currency.ShouldBeNull();
        error.ShouldNotBeNull();
        error.Code.ShouldBe(DomainErrorCodes.CurrencyRequired);
    }

    [Theory]
    [InlineData("CH")]
    [InlineData("CHFF")]
    [InlineData("12!")]
    public void TryCreate_WithInvalidCodeFormat_ReturnsCurrencyInvalidFormat(string code)
    {
        // Act
        bool result = Currency.TryCreate(
            code,
            out Currency? currency,
            out DomainError? error);

        // Assert
        result.ShouldBeFalse();
        currency.ShouldBeNull();
        error.ShouldNotBeNull();
        error.Code.ShouldBe(DomainErrorCodes.CurrencyInvalidFormat);
    }

    [Fact]
    public void TryCreate_WithUnsupportedCode_ReturnsCurrencyUnsupported()
    {
        // Act
        bool result = Currency.TryCreate(
            "ABC",
            out Currency? currency,
            out DomainError? error);

        // Assert
        result.ShouldBeFalse();
        currency.ShouldBeNull();
        error.ShouldNotBeNull();
        error.Code.ShouldBe(DomainErrorCodes.CurrencyUnsupported);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void TryCreate_WithMissingSymbol_ReturnsCurrencySymbolRequired(string? symbol)
    {
        // Act
        bool result = Currency.TryCreate(
            "CHF",
            symbol,
            out Currency? currency,
            out DomainError? error);

        // Assert
        result.ShouldBeFalse();
        currency.ShouldBeNull();
        error.ShouldNotBeNull();
        error.Code.ShouldBe(DomainErrorCodes.CurrencySymbolRequired);
    }

    [Theory]
    [InlineData("CH")]
    [InlineData("CHFF")]
    [InlineData("12!")]
    public void TryCreate_WithSymbolAndInvalidCodeFormat_ReturnsCurrencyInvalidFormat(string code)
    {
        // Act
        bool result = Currency.TryCreate(
            code,
            "Fr.",
            out Currency? currency,
            out DomainError? error);

        // Assert
        result.ShouldBeFalse();
        currency.ShouldBeNull();
        error.ShouldNotBeNull();
        error.Code.ShouldBe(DomainErrorCodes.CurrencyInvalidFormat);
    }

    [Fact]
    public void TryCreate_WithSymbolAndUnsupportedCode_ReturnsCurrencyUnsupported()
    {
        // Act
        bool result = Currency.TryCreate(
            "ABC",
            "ABC",
            out Currency? currency,
            out DomainError? error);

        // Assert
        result.ShouldBeFalse();
        currency.ShouldBeNull();
        error.ShouldNotBeNull();
        error.Code.ShouldBe(DomainErrorCodes.CurrencyUnsupported);
    }
}