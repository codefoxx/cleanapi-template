// ReSharper disable once CheckNamespace

namespace Company.Template.Domain.Common;

public static partial class DomainErrorCodes
{
    public static readonly DomainErrorCode AmountNegative =
        DomainErrorCode.Create("amount_negative");

    public static readonly DomainErrorCode AmountTooManyDecimalPlaces =
        DomainErrorCode.Create("amount_too_many_decimal_places");

    public static readonly DomainErrorCode CurrencyInvalidFormat =
        DomainErrorCode.Create("currency_invalid_format");

    public static readonly DomainErrorCode CurrencyRequired =
        DomainErrorCode.Create("currency_required");

    public static readonly DomainErrorCode CurrencySymbolRequired =
        DomainErrorCode.Create("currency_symbol_required");

    public static readonly DomainErrorCode DiscontinuedProductCannotBeChanged =
        DomainErrorCode.Create("discontinued_product_cannot_be_changed");

    public static readonly DomainErrorCode ProductIdRequired =
        DomainErrorCode.Create("product_id_required");

    public static readonly DomainErrorCode ProductNameRequired =
        DomainErrorCode.Create("product_name_required");

    public static readonly DomainErrorCode ProductNameTooLong =
        DomainErrorCode.Create("product_name_too_long");
}
