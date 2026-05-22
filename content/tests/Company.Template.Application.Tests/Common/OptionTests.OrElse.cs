using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class OptionTests
{
    [Fact]
    public void OrElse_WithSome_ReturnsValue()
    {
        // Arrange
        Option<string> option = Option.Some("value");

        // Act
        string result = option.OrElse("fallback");

        // Assert
        result.ShouldBe("value");
    }

    [Fact]
    public void OrElse_WithNone_ReturnsFallback()
    {
        // Arrange
        Option<string> option = Option.None<string>();

        // Act
        string result = option.OrElse("fallback");

        // Assert
        result.ShouldBe("fallback");
    }

    [Fact]
    public void OrElseFactory_WithSome_DoesNotInvokeFallbackFactory()
    {
        // Arrange
        Option<string> option = Option.Some("value");
        bool fallbackWasCalled = false;

        // Act
        string result = option.OrElse(() =>
        {
            fallbackWasCalled = true;
            return "fallback";
        });

        // Assert
        result.ShouldBe("value");
        fallbackWasCalled.ShouldBeFalse();
    }

    [Fact]
    public void OrElseFactory_WithNone_ReturnsFallbackFactoryValue()
    {
        // Arrange
        Option<string> option = Option.None<string>();

        // Act
        string result = option.OrElse(() => "fallback");

        // Assert
        result.ShouldBe("fallback");
    }

    [Fact]
    public void OrElseFactory_WithNullFallbackFactory_ThrowsArgumentNullException()
    {
        // Arrange
        Option<string> option = Option.None<string>();
        Func<string> fallback = null!;

        // Act
        Action action = () => option.OrElse(fallback);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
