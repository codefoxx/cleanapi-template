using Company.Template.Domain.Products;

namespace Company.Template.Domain.Tests.Products;

public sealed class KnownCurrenciesTests
{
    [Fact]
    public void Chf_ReturnsSwissFranc()
    {
        // Act
        Currency currency = KnownCurrencies.Chf;

        // Assert
        currency.Code.ShouldBe("CHF");
        currency.Symbol.ShouldBe("CHF");
    }

    [Fact]
    public void Eur_ReturnsEuro()
    {
        // Act
        Currency currency = KnownCurrencies.Eur;

        // Assert
        currency.Code.ShouldBe("EUR");
        currency.Symbol.ShouldBe("€");
    }

    [Fact]
    public void Usd_ReturnsUsDollar()
    {
        // Act
        Currency currency = KnownCurrencies.Usd;

        // Assert
        currency.Code.ShouldBe("USD");
        currency.Symbol.ShouldBe("$");
    }
}
