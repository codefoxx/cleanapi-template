using Company.Template.Domain.Common;

namespace Company.Template.Application.Common;

public static class DomainErrorExtensions
{
    private static readonly IReadOnlyDictionary<DomainErrorCode, ErrorType> ErrorTypes =
        new Dictionary<DomainErrorCode, ErrorType>
        {
            [DomainErrorCodes.AmountNegative] = ErrorType.Validation,
            [DomainErrorCodes.AmountTooManyDecimalPlaces] = ErrorType.Validation,
            [DomainErrorCodes.Conflict] = ErrorType.Conflict,
            [DomainErrorCodes.CurrencyInvalidFormat] = ErrorType.Validation,
            [DomainErrorCodes.CurrencyRequired] = ErrorType.Validation,
            [DomainErrorCodes.CurrencySymbolRequired] = ErrorType.Validation,
            [DomainErrorCodes.CurrencyUnsupported] = ErrorType.Validation,
            [DomainErrorCodes.DiscontinuedProductCannotBeChanged] = ErrorType.Conflict,
            [DomainErrorCodes.NotFound] = ErrorType.Validation,
            [DomainErrorCodes.ProductIdRequired] = ErrorType.Validation,
            [DomainErrorCodes.ProductNameRequired] = ErrorType.Validation,
            [DomainErrorCodes.ProductNameTooLong] = ErrorType.Validation,
            [DomainErrorCodes.ValidationError] = ErrorType.Validation
        };

    public static Error ToApplicationError(this DomainError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        ErrorType type = ErrorTypes.TryGetValue(error.Code, out ErrorType mappedType)
            ? mappedType
            : ErrorType.Unknown;

        return Error.Create(type, ErrorCode.FromDomain(error.Code), error.Message);
    }
}
