using Company.Template.Application.Common;
using Company.Template.Application.Common.Validation;

using TemplateValidation = Company.Template.Application.Common.Validation.Validation;

namespace Company.Template.Application.Tests.Common.Validation;

public sealed class ValidationTests
{
    [Fact]
    public void For_WithNullValue_ThrowsArgumentNullException()
    {
        // Act
        Action action = () => TemplateValidation.For<string>(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Map_WhenAllRulesPass_ReturnsMappedValue()
    {
        // Arrange
        TestRequest request = new("Valid name", 42);

        // Act
        ValidationResult<string> result = TemplateValidation.For(request)
            .Rule(value => value.Name.Length > 0
                ? null
                : Error.Validation("Name is required."))
            .RuleFor(value => value.Amount, amount => amount > 0
                ? null
                : Error.Validation("Amount must be positive."))
            .Map(value => value.Name);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe("Valid name");
    }

    [Fact]
    public void Map_WhenRuleFails_DoesNotInvokeMapper()
    {
        // Arrange
        TestRequest request = new("", 42);
        bool mapperWasCalled = false;

        // Act
        ValidationResult<string> result = TemplateValidation.For(request)
            .Rule(value => string.IsNullOrWhiteSpace(value.Name)
                ? Error.Validation("Name is required.")
                : null)
            .Map(_ =>
            {
                mapperWasCalled = true;
                return "mapped";
            });

        // Assert
        result.IsValid.ShouldBeFalse();
        mapperWasCalled.ShouldBeFalse();
    }

    [Fact]
    public void Map_WhenMultipleRulesFail_CollectsAllErrors()
    {
        // Arrange
        TestRequest request = new("", -1);

        // Act
        ValidationResult<string> result = TemplateValidation.For(request)
            .Rule(value => string.IsNullOrWhiteSpace(value.Name)
                ? Error.Validation("Name is required.")
                : null)
            .RuleFor(value => value.Amount, amount => amount < 0
                ? Error.Validation("Amount cannot be negative.")
                : null)
            .Map(value => value.Name);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(2);
        result.Errors.Select(error => error.Message)
            .ShouldBe(["Name is required.", "Amount cannot be negative."]);
    }

    [Fact]
    public void RuleFor_WhenRuleFails_AddsCamelCasePropertyNameAsTarget()
    {
        // Arrange
        TestRequest request = new("Valid name", -1);

        // Act
        ValidationResult<string> result = TemplateValidation.For(request)
            .RuleFor(value => value.Amount, amount => amount < 0
                ? Error.Validation("Amount cannot be negative.")
                : null)
            .Map(value => value.Name);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Single().Target.ShouldBe("amount");
    }

    [Fact]
    public void RuleFor_WhenErrorAlreadyHasTarget_PreservesExistingTarget()
    {
        // Arrange
        TestRequest request = new("Valid name", -1);

        // Act
        ValidationResult<string> result = TemplateValidation.For(request)
            .RuleFor(value => value.Amount, amount => amount < 0
                ? Error.Validation(ErrorCodes.ValidationError, "Amount cannot be negative.", "customAmount")
                : null)
            .Map(value => value.Name);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Single().Target.ShouldBe("customAmount");
    }

    [Fact]
    public void RuleFor_WithComplexSelector_ThrowsArgumentException()
    {
        // Arrange
        TestRequest request = new("Valid name", 42);

        // Act
        Action action = () => TemplateValidation.For(request)
            .RuleFor(value => value.Name.Length, _ => null);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Rule_WithNullRule_ThrowsArgumentNullException()
    {
        // Arrange
        TestRequest request = new("Valid name", 42);
        Func<TestRequest, Error?> rule = null!;

        // Act
        Action action = () => TemplateValidation.For(request).Rule(rule);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void RuleFor_WithNullSelector_ThrowsArgumentNullException()
    {
        // Arrange
        TestRequest request = new("Valid name", 42);
        Func<int, Error?> rule = _ => null;

        // Act
        Action action = () => TemplateValidation.For(request).RuleFor(null!, rule);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void RuleFor_WithNullRule_ThrowsArgumentNullException()
    {
        // Arrange
        TestRequest request = new("Valid name", 42);
        Func<int, Error?> rule = null!;

        // Act
        Action action = () => TemplateValidation.For(request).RuleFor(value => value.Amount, rule);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Map_WithNullMapper_ThrowsArgumentNullException()
    {
        // Arrange
        TestRequest request = new("Valid name", 42);
        Func<TestRequest, string> map = null!;

        // Act
        Action action = () => TemplateValidation.For(request).Map(map);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    private sealed record TestRequest(string Name, int Amount);
}
