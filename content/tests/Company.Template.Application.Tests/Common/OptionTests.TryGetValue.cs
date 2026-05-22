using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class OptionTests
{
    [Fact]
    public void TryGetValue_WithSome_ReturnsTrueAndValue()
    {
        // Arrange
        Option<string> option = Option.Some("value");

        // Act
        bool result = option.TryGetValue(out string? value);

        // Assert
        result.ShouldBeTrue();
        value.ShouldBe("value");
    }

    [Fact]
    public void TryGetValue_WithNone_ReturnsFalseAndDefault()
    {
        // Arrange
        Option<string> option = Option.None<string>();

        // Act
        bool result = option.TryGetValue(out string? value);

        // Assert
        result.ShouldBeFalse();
        value.ShouldBeNull();
    }
}
