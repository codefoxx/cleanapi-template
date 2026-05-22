using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class OptionTests
{
    [Fact]
    public void Where_WithPredicateTrue_ReturnsSameOption()
    {
        // Arrange
        Option<int> option = Option.Some(21);

        // Act
        Option<int> result = option.Where(value => value > 0);

        // Assert
        result.ShouldBe(option);
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
    public void Where_WithNone_DoesNotInvokePredicate()
    {
        // Arrange
        Option<int> option = Option.None<int>();
        bool predicateWasCalled = false;

        // Act
        Option<int> result = option.Where(_ =>
        {
            predicateWasCalled = true;
            return true;
        });

        // Assert
        result.IsNone.ShouldBeTrue();
        predicateWasCalled.ShouldBeFalse();
    }

    [Fact]
    public void Where_WithNullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        Option<int> option = Option.Some(21);
        Func<int, bool> predicate = null!;

        // Act
        Action action = () => option.Where(predicate);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
