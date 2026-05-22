using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class OptionTests
{
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
    public void Bind_WithNone_ReturnsNone()
    {
        // Arrange
        Option<int> option = Option.None<int>();

        // Act
        Option<int> result = option.Bind(value => Option.Some(value * 2));

        // Assert
        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Bind_WithNone_DoesNotInvokeBinder()
    {
        // Arrange
        Option<int> option = Option.None<int>();
        bool binderWasCalled = false;

        // Act
        Option<int> result = option.Bind(_ =>
        {
            binderWasCalled = true;
            return Option.Some(42);
        });

        // Assert
        result.IsNone.ShouldBeTrue();
        binderWasCalled.ShouldBeFalse();
    }

    [Fact]
    public void Bind_WithNullDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Option<int> option = Option.Some(21);
        Func<int, Option<int>> bind = null!;

        // Act
        Action action = () => option.Bind(bind);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
