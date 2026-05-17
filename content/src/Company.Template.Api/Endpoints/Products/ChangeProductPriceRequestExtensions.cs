using Company.Template.Application.Common;
using Company.Template.Application.Common.Validation;
using Company.Template.Application.Products.ChangeProductPrice;

namespace Company.Template.Api.Endpoints.Products;

internal static class ChangeProductPriceRequestExtensions
{
    public static Result<ChangeProductPriceCommand> ToCommand(
        this ChangeProductPriceRequest request,
        Guid productId)
    {
        return Validation
              .For(request)
              .RuleFor(x => x.Price, ValidatePrice)
              .RuleFor(x => x.Currency, ValidateCurrency)
              .Map(validRequest => new ChangeProductPriceCommand(
                   productId,
                   validRequest.Price,
                   validRequest.Currency!.Trim()))
              .ToResult();
    }

    private static Error? ValidatePrice(decimal price)
    {
        return price < 0
            ? Error.Validation(
                ErrorCodes.AmountNegative,
                "Price cannot be negative.")
            : null;
    }

    private static Error? ValidateCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? Error.Validation(
                ErrorCodes.CurrencyRequired,
                "Currency is required.")
            : null;
    }
}