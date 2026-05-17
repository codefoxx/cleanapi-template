using Company.Template.Application.Common;
using Company.Template.Application.Common.Validation;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Domain.Common;

namespace Company.Template.Api.Endpoints.Products;

internal static class CreateProductRequestExtensions
{
    public static Result<CreateProductCommand> ToCommand(this CreateProductRequest request)
    {
        return Validation.For(request)
                         .RuleFor(x => x.Name, ValidateName)
                         .RuleFor(x => x.Price, ValidatePrice)
                         .RuleFor(x => x.Currency, ValidateCurrency)
                         .Map(CreateCommand)
                         .ToResult();
    }

    private static CreateProductCommand CreateCommand(CreateProductRequest request)
    {
        return new CreateProductCommand(
            request.Name.Trim(),
            request.Price,
            request.Currency.Trim());
    }

    private static Error? ValidateName(string? name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? Error.Validation(
                DomainErrorCodes.ProductNameRequired,
                "Product name is required.")
            : null;
    }

    private static Error? ValidatePrice(decimal price)
    {
        return price < 0
            ? Error.Validation(
                DomainErrorCodes.AmountNegative,
                "Price cannot be negative.")
            : null;
    }

    private static Error? ValidateCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? Error.Validation(
                DomainErrorCodes.CurrencyRequired,
                "Currency is required.")
            : null;
    }
}
