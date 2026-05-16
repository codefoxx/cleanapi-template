using Company.Template.Domain.Common;

namespace Company.Template.Application.Common;

public static class DomainErrorExtensions
{
    private static readonly IReadOnlyDictionary<DomainErrorCode, ErrorType> ErrorTypes =
        new Dictionary<DomainErrorCode, ErrorType>
        {
            [DomainErrorCodes.AmountNegative] = ErrorType.Validation,
            [DomainErrorCodes.AmountTooManyDecimalPlaces] = ErrorType.Validation,
            [DomainErrorCodes.CurrencyInvalidFormat] = ErrorType.Validation,
            [DomainErrorCodes.CurrencyRequired] = ErrorType.Validation,
            [DomainErrorCodes.CurrencySymbolRequired] = ErrorType.Validation,
            [DomainErrorCodes.CurrencyUnsupported] = ErrorType.Validation,
            [DomainErrorCodes.DiscontinuedProductCannotBeChanged] = ErrorType.Conflict,
            [DomainErrorCodes.ProductIdRequired] = ErrorType.Validation,
            [DomainErrorCodes.ProductNameRequired] = ErrorType.Validation,
            [DomainErrorCodes.ProductNameTooLong] = ErrorType.Validation,
        };

    public static Error ToApplicationError(this DomainError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        ErrorType type = ErrorTypes.TryGetValue(error.Code, out ErrorType mappedType)
            ? mappedType
            : ErrorType.Unknown;

        return Error.Create(type, error.Code.Value, error.Message);
    }
}
