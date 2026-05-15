using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed class OptionTests
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

    [Fact]
    public void Match_WithSome_CallsSomeBranch()
    {
        // Arrange
        Option<string> option = Option.Some("value");

        // Act
        string result = option.Match(
            value => $"some:{value}",
            () => "none");

        // Assert
        result.ShouldBe("some:value");
    }

    [Fact]
    public void Match_WithNone_CallsNoneBranch()
    {
        // Arrange
        Option<string> option = Option.None<string>();

        // Act
        string result = option.Match(
            value => $"some:{value}",
            () => "none");

        // Assert
        result.ShouldBe("none");
    }

    [Fact]
    public void Map_WithSome_TransformsValue()
    {
        // Arrange
        Option<int> option = Option.Some(21);

        // Act
        Option<int> result = option.Map(value => value * 2);

        // Assert
        result.HasValue.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void Map_WithNone_ReturnsNone()
    {
        // Arrange
        Option<int> option = Option.None<int>();

        // Act
        Option<int> result = option.Map(value => value * 2);

        // Assert
        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Bind_WithSome_ReturnsBoundOption()
    {
        // Arrange
        Option<int> option = Option.Some(21);

        // Act
        Option<int> result = option.Bind(value => Option.Some(value * 2));

        // Assert
        result.HasValue.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void Where_WithPredicateFalse_ReturnsNone()
    {
        // Arrange
        Option<int> option = Option.Some(21);

        // Act
        Option<int> result = option.Where(value => value > 100);

        // Assert
        result.IsNone.ShouldBeTrue();
    }

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
