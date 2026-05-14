using Company.Template.Domain.Products;

namespace Company.Template.Domain.Tests.Products;

public sealed class MoneyTests
{
    private static readonly Currency Chf = KnownCurrencies.Chf;
    private static readonly Currency Eur = KnownCurrencies.Eur;

    [Fact]
    public void Create_WithPositiveAmountAndCurrency_ReturnsMoney()
    {
        // Arrange
        const decimal amount = 12.50m;
        Currency currency = Chf;

        // Act
        Money money = Money.Create(amount, currency);

        // Assert
        money.Amount.ShouldBe(amount);
        money.Currency.ShouldBe(currency);
        money.IsZero.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithZeroAmountAndEmptyCurrency_ReturnsZeroMoney()
    {
        // Arrange
        const decimal amount = 0m;
        Currency currency = Currency.Empty;

        // Act
        Money money = Money.Create(amount, currency);

        // Assert
        money.Amount.ShouldBe(0m);
        money.Currency.ShouldBe(Currency.Empty);
        money.IsZero.ShouldBeTrue();
    }

    [Fact]
    public void Create_WithZeroAmountAndCurrency_ReturnsMoneyWithCurrency()
    {
        // Arrange
        const decimal amount = 0m;
        Currency currency = Chf;

        // Act
        Money money = Money.Create(amount, currency);

        // Assert
        money.Amount.ShouldBe(0m);
        money.Currency.ShouldBe(currency);
        money.IsZero.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithNegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const decimal amount = -0.01m;

        // Act
        ArgumentOutOfRangeException exception =
            Should.Throw<ArgumentOutOfRangeException>(() => Money.Create(amount, Chf));

        // Assert
        exception.Message.ShouldStartWith("Price cannot be negative.");
        exception.ParamName.ShouldBe("amount");
    }

    [Fact]
    public void Create_WithPositiveAmountAndEmptyCurrency_ThrowsArgumentException()
    {
        // Arrange
        const decimal amount = 1m;

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Money.Create(amount, Currency.Empty));

        // Assert
        exception.Message.ShouldStartWith("Currency is required when amount is greater than zero.");
        exception.ParamName.ShouldBe("currency");
    }

    [Fact]
    public void Create_WithTooManyDecimalPlaces_ThrowsArgumentException()
    {
        // Arrange
        const decimal amount = 1.001m;

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => Money.Create(amount, Chf));

        // Assert
        exception.Message.ShouldStartWith($"Price cannot have more than {Money.Scale} decimal places.");
        exception.ParamName.ShouldBe("amount");
    }

    [Fact]
    public void Create_WithNullCurrency_ThrowsArgumentNullException()
    {
        // Arrange
        Currency currency = null!;

        // Act
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => Money.Create(1m, currency));

        // Assert
        exception.ParamName.ShouldBe("currency");
    }

    [Fact]
    public void Create_WithStringCurrency_CreatesMoneyWithCurrency()
    {
        // Arrange
        const decimal amount = 12.50m;
        const string currency = "chf";

        // Act
        Money money = Money.Create(amount, currency);

        // Assert
        money.Amount.ShouldBe(amount);
        money.Currency.ShouldBe(Chf);
    }

    [Fact]
    public void CreateRounded_WithTooManyDecimalPlaces_RoundsAwayFromZero()
    {
        // Arrange
        const decimal amount = 1.005m;

        // Act
        Money money = Money.CreateRounded(amount, Chf);

        // Assert
        money.Amount.ShouldBe(1.01m);
        money.Currency.ShouldBe(Chf);
    }

    [Fact]
    public void CreateRounded_WithNegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const decimal amount = -1.005m;

        // Act
        ArgumentOutOfRangeException exception =
            Should.Throw<ArgumentOutOfRangeException>(() => Money.CreateRounded(amount, Chf));

        // Assert
        exception.Message.ShouldStartWith("Price cannot be negative.");
        exception.ParamName.ShouldBe("amount");
    }

    [Fact]
    public void CreateRounded_WithNullCurrency_ThrowsArgumentNullException()
    {
        // Arrange
        Currency currency = null!;

        // Act
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => Money.CreateRounded(1m, currency));

        // Assert
        exception.ParamName.ShouldBe("currency");
    }

    [Fact]
    public void Zero_WithoutCurrency_ReturnsZeroWithEmptyCurrency()
    {
        // Act
        Money money = Money.Zero();

        // Assert
        money.Amount.ShouldBe(0m);
        money.Currency.ShouldBe(Currency.Empty);
        money.IsZero.ShouldBeTrue();
    }

    [Fact]
    public void Zero_WithCurrency_ReturnsZeroWithCurrency()
    {
        // Arrange
        Currency currency = Chf;

        // Act
        Money money = Money.Zero(currency);

        // Assert
        money.Amount.ShouldBe(0m);
        money.Currency.ShouldBe(currency);
        money.IsZero.ShouldBeFalse();
    }

    [Fact]
    public void Zero_WithNullCurrency_ThrowsArgumentNullException()
    {
        // Arrange
        Currency currency = null!;

        // Act
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => Money.Zero(currency));

        // Assert
        exception.ParamName.ShouldBe("currency");
    }

    [Fact]
    public void Add_WithSameCurrency_ReturnsSum()
    {
        // Arrange
        Money left = Money.Create(10m, Chf);
        Money right = Money.Create(2.50m, Chf);

        // Act
        Money result = left.Add(right);

        // Assert
        result.ShouldBe(Money.Create(12.50m, Chf));
    }

    [Fact]
    public void Add_WithDifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        Money left = Money.Create(10m, Chf);
        Money right = Money.Create(2.50m, Eur);

        // Act
        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => left.Add(right));

        // Assert
        exception.Message.ShouldStartWith("Cannot operate on money values with different currencies:");
    }

    [Fact]
    public void Add_WithNullMoney_ThrowsArgumentNullException()
    {
        // Arrange
        Money left = Money.Create(10m, Chf);
        Money right = null!;

        // Act
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => left.Add(right));

        // Assert
        exception.ParamName.ShouldBe("other");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ReturnsDifference()
    {
        // Arrange
        Money left = Money.Create(10m, Chf);
        Money right = Money.Create(2.50m, Chf);

        // Act
        Money result = left.Subtract(right);

        // Assert
        result.ShouldBe(Money.Create(7.50m, Chf));
    }

    [Fact]
    public void Subtract_WhenResultWouldBeNegative_ThrowsInvalidOperationException()
    {
        // Arrange
        Money left = Money.Create(2.50m, Chf);
        Money right = Money.Create(10m, Chf);

        // Act
        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => left.Subtract(right));

        // Assert
        exception.Message.ShouldBe("Cannot subtract more than the current amount.");
    }

    [Fact]
    public void Subtract_WithDifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        Money left = Money.Create(10m, Chf);
        Money right = Money.Create(2.50m, Eur);

        // Act
        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => left.Subtract(right));

        // Assert
        exception.Message.ShouldStartWith("Cannot operate on money values with different currencies:");
    }

    [Fact]
    public void Subtract_WithNullMoney_ThrowsArgumentNullException()
    {
        // Arrange
        Money left = Money.Create(10m, Chf);
        Money right = null!;

        // Act
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => left.Subtract(right));

        // Assert
        exception.ParamName.ShouldBe("other");
    }

    [Fact]
    public void ScaleBy_WithPositiveFactor_ReturnsScaledMoney()
    {
        // Arrange
        Money money = Money.Create(10m, Chf);
        const decimal factor = 1.5m;

        // Act
        Money result = money.ScaleBy(factor);

        // Assert
        result.ShouldBe(Money.Create(15m, Chf));
    }

    [Fact]
    public void ScaleBy_WithFractionalResult_RoundsAwayFromZero()
    {
        // Arrange
        Money money = Money.Create(10.01m, Chf);
        const decimal factor = 0.5m;

        // Act
        Money result = money.ScaleBy(factor);

        // Assert
        result.ShouldBe(Money.Create(5.01m, Chf));
    }

    [Fact]
    public void ScaleBy_WithZeroFactor_ReturnsZeroWithSameCurrency()
    {
        // Arrange
        Money money = Money.Create(10m, Chf);
        const decimal factor = 0m;

        // Act
        Money result = money.ScaleBy(factor);

        // Assert
        result.ShouldBe(Money.Zero(Chf));
    }

    [Fact]
    public void ScaleBy_WithNegativeFactor_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        Money money = Money.Create(10m, Chf);
        const decimal factor = -1m;

        // Act
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() => money.ScaleBy(factor));

        // Assert
        exception.Message.ShouldStartWith("Factor cannot be negative.");
        exception.ParamName.ShouldBe("factor");
    }

    [Fact]
    public void CompareTo_WithLowerAmountInSameCurrency_ReturnsNegativeValue()
    {
        // Arrange
        Money lower = Money.Create(1m, Chf);
        Money higher = Money.Create(2m, Chf);

        // Act
        int result = lower.CompareTo(higher);

        // Assert
        result.ShouldBeLessThan(0);
    }

    [Fact]
    public void CompareTo_WithHigherAmountInSameCurrency_ReturnsPositiveValue()
    {
        // Arrange
        Money lower = Money.Create(1m, Chf);
        Money higher = Money.Create(2m, Chf);

        // Act
        int result = higher.CompareTo(lower);

        // Assert
        result.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CompareTo_WithSameAmountAndSameCurrency_ReturnsZero()
    {
        // Arrange
        Money left = Money.Create(1m, Chf);
        Money right = Money.Create(1m, Chf);

        // Act
        int result = left.CompareTo(right);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public void CompareTo_WithNull_ReturnsPositiveValue()
    {
        // Arrange
        Money money = Money.Create(1m, Chf);
        Money other = null!;

        // Act
        int result = money.CompareTo(other);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public void CompareTo_WithDifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        Money left = Money.Create(1m, Chf);
        Money right = Money.Create(1m, Eur);

        // Act
        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => left.CompareTo(right));

        // Assert
        exception.Message.ShouldStartWith("Cannot operate on money values with different currencies:");
    }

    [Fact]
    public void CompareTo_WithEmptyZeroAndDifferentCurrency_ComparesAmountWithoutCurrencyCheck()
    {
        // Arrange
        Money zero = Money.Zero();
        Money other = Money.Create(1m, Eur);

        // Act
        int result = zero.CompareTo(other);

        // Assert
        result.ShouldBeLessThan(0);
    }

    [Fact]
    public void PlusOperator_WithSameCurrency_ReturnsSum()
    {
        // Arrange
        Money left = Money.Create(10m, Chf);
        Money right = Money.Create(2.50m, Chf);

        // Act
        Money result = left + right;

        // Assert
        result.ShouldBe(Money.Create(12.50m, Chf));
    }

    [Fact]
    public void MinusOperator_WithSameCurrency_ReturnsDifference()
    {
        // Arrange
        Money left = Money.Create(10m, Chf);
        Money right = Money.Create(2.50m, Chf);

        // Act
        Money result = left - right;

        // Assert
        result.ShouldBe(Money.Create(7.50m, Chf));
    }

    [Fact]
    public void LessThanOperator_WithLowerAmountInSameCurrency_ReturnsTrue()
    {
        // Arrange
        Money lower = Money.Create(1m, Chf);
        Money higher = Money.Create(2m, Chf);

        // Act
        bool result = lower < higher;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void LessThanOrEqualOperator_WithSameAmountInSameCurrency_ReturnsTrue()
    {
        // Arrange
        Money left = Money.Create(1m, Chf);
        Money right = Money.Create(1m, Chf);

        // Act
        bool result = left <= right;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void GreaterThanOperator_WithHigherAmountInSameCurrency_ReturnsTrue()
    {
        // Arrange
        Money lower = Money.Create(1m, Chf);
        Money higher = Money.Create(2m, Chf);

        // Act
        bool result = higher > lower;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void GreaterThanOrEqualOperator_WithSameAmountInSameCurrency_ReturnsTrue()
    {
        // Arrange
        Money left = Money.Create(1m, Chf);
        Money right = Money.Create(1m, Chf);

        // Act
        bool result = left >= right;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ToString_WithCurrency_ReturnsAmountAndCurrencyCode()
    {
        // Arrange
        Money money = Money.Create(12.5m, Chf);

        // Act
        string value = money.ToString();

        // Assert
        value.ShouldBe("12.50 CHF");
    }

    [Fact]
    public void ToString_WithEmptyCurrency_ReturnsAmountOnly()
    {
        // Arrange
        Money money = Money.Zero();

        // Act
        string value = money.ToString();

        // Assert
        value.ShouldBe("0.00");
    }

    [Fact]
    public void TryCreate_WithValidAmountAndCurrency_ReturnsTrueAndMoney()
    {
        // Act
        bool result = Money.TryCreate(
            99.90m,
            KnownCurrencies.Chf,
            out Money? money,
            out DomainError? error);

        // Assert
        result.ShouldBeTrue();
        money.ShouldNotBeNull();
        money.Amount.ShouldBe(99.90m);
        money.Currency.ShouldBe(KnownCurrencies.Chf);
        error.ShouldBeNull();
    }

    [Fact]
    public void TryCreate_WithNegativeAmount_ReturnsFalseAndDomainError()
    {
        // Act
        bool result = Money.TryCreate(
            -0.01m,
            KnownCurrencies.Chf,
            out Money? money,
            out DomainError? error);

        // Assert
        result.ShouldBeFalse();
        money.ShouldBeNull();
        error.ShouldNotBeNull();
        error.Code.ShouldBe(DomainErrorCodes.AmountNegative);
        error.Message.ShouldBe("Amount cannot be negative.");
    }

    [Fact]
    public void TryCreate_WithTooManyDecimalPlaces_ReturnsFalseAndDomainError()
    {
        // Act
        bool result = Money.TryCreate(
            99.999m,
            KnownCurrencies.Chf,
            out Money? money,
            out DomainError? error);

        // Assert
        result.ShouldBeFalse();
        money.ShouldBeNull();
        error.ShouldNotBeNull();
        error.Code.ShouldBe(DomainErrorCodes.AmountTooManyDecimalPlaces);
        error.Message.ShouldBe($"Price cannot have more than {Money.Scale} decimal places.");
    }

    [Fact]
    public void TryCreate_WithInvalidCurrencyCode_ReturnsFalseAndDomainError()
    {
        // Act
        bool result = Money.TryCreate(
            99.90m,
            "CH",
            out Money? money,
            out DomainError? error);

        // Assert
        result.ShouldBeFalse();
        money.ShouldBeNull();
        error.ShouldNotBeNull();
        error.Code.ShouldBe(DomainErrorCodes.CurrencyInvalidFormat);
        error.Message.ShouldBe("Currency must be an ISO 4217 three-letter code.");
    }
}
