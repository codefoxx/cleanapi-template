using Company.Template.Domain.SharedKernel;

namespace Company.Template.Domain.Tests.SharedKernel;

public sealed class Iso4217CurrencyCodesTests
{
    [Fact]
    public void Chf_ReturnsSwissFranc()
    {
        // Act
        Currency currency = Iso4217CurrencyCodes.Chf;

        // Assert
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("CHF");
    }

    [Fact]
    public void Eur_ReturnsEuro()
    {
        // Act
        Currency currency = Iso4217CurrencyCodes.Eur;

        // Assert
        currency.Code.ShouldBe("EUR");
        currency.Symbol.ShouldBe("€");
    }

    [Fact]
    public void Usd_ReturnsUsDollar()
    {
        // Act
        Currency currency = Iso4217CurrencyCodes.Usd;

        // Assert
        currency.Code.ShouldBe("USD");
        currency.Symbol.ShouldBe("$");
    }
}
