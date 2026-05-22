using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class OptionTests
{
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
    public void Match_WithNullSomeDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Option<string> option = Option.Some("value");
        Func<string, string> some = null!;

        // Act
        Action action = () => option.Match(some, () => "none");

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Match_WithNullNoneDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Option<string> option = Option.None<string>();
        Func<string> none = null!;

        // Act
        Action action = () => option.Match(value => $"some:{value}", none);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
