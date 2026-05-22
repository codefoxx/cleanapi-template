using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class OptionTests
{
    [Fact]
    public void FromNullable_WithNonNullReference_ReturnsSome()
    {
        // Arrange
        string? value = "value";

        // Act
        Option<string> option = Option.FromNullable(value);

        // Assert
        option.HasValue.ShouldBeTrue();
        option.Value.ShouldBe("value");
    }

    [Fact]
    public void FromNullable_WithNullReference_ReturnsNone()
    {
        // Arrange
        string? value = null;

        // Act
        Option<string> option = Option.FromNullable(value);

        // Assert
        option.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void FromNullable_WithNullableStructValue_ReturnsSome()
    {
        // Arrange
        int? value = 42;

        // Act
        Option<int> option = Option.FromNullable(value);

        // Assert
        option.HasValue.ShouldBeTrue();
        option.Value.ShouldBe(42);
    }

    [Fact]
    public void FromNullable_WithNullNullableStruct_ReturnsNone()
    {
        // Arrange
        int? value = null;

        // Act
        Option<int> option = Option.FromNullable(value);

        // Assert
        option.IsNone.ShouldBeTrue();
    }
}
