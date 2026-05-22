using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class OptionTests
{
    [Fact]
    public void Some_WithValue_ReturnsOptionWithValue()
    {
        // Act
        Option<string> option = Option.Some("value");

        // Assert
        option.HasValue.ShouldBeTrue();
        option.IsNone.ShouldBeFalse();
        option.Value.ShouldBe("value");
    }

    [Fact]
    public void Some_WithNullValue_ThrowsArgumentNullException()
    {
        // Act
        Action action = () => Option.Some<string>(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void None_ReturnsOptionWithoutValue()
    {
        // Act
        Option<string> option = Option.None<string>();

        // Assert
        option.HasValue.ShouldBeFalse();
        option.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Value_OnNone_ThrowsInvalidOperationException()
    {
        // Arrange
        Option<string> option = Option.None<string>();

        // Act
        Action action = () => _ = option.Value;

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }
}
