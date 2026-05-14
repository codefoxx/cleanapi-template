using Company.Template.Domain.Common;

namespace Company.Template.Application.Common;

public static class DomainErrorExtensions
{
    public static Error ToApplicationError(this DomainError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return error.Code switch
        {
            var code when code == DomainErrorCodes.ProductNameRequired =>
                Error.Validation(code.Value, error.Message),

            var code when code == DomainErrorCodes.ProductNameTooLong =>
                Error.Validation(code.Value, error.Message),

            var code when code == DomainErrorCodes.CurrencyRequired =>
                Error.Validation(code.Value, error.Message),

            var code when code == DomainErrorCodes.CurrencyInvalidFormat =>
                Error.Validation(code.Value, error.Message),

            var code when code == DomainErrorCodes.CurrencySymbolRequired =>
                Error.Validation(code.Value, error.Message),

            var code when code == DomainErrorCodes.AmountNegative =>
                Error.Validation(code.Value, error.Message),

            var code when code == DomainErrorCodes.AmountTooManyDecimalPlaces =>
                Error.Validation(code.Value, error.Message),

            var code when code == DomainErrorCodes.DiscontinuedProductCannotBeChanged =>
                Error.Conflict(code.Value, error.Message),

            _ => Error.Unknown(error.Code.Value, error.Message)
        };
    }
}
