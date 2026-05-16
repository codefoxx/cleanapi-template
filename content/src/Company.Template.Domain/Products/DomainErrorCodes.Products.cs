// ReSharper disable once CheckNamespace

namespace Company.Template.Domain.Common;

public static partial class DomainErrorCodes
{
    public static readonly DomainErrorCode DiscontinuedProductCannotBeChanged =
        DomainErrorCode.Create("discontinued_product_cannot_be_changed");

    public static readonly DomainErrorCode ProductIdRequired =
        DomainErrorCode.Create("product_id_required");

    public static readonly DomainErrorCode ProductNameRequired =
        DomainErrorCode.Create("product_name_required");

    public static readonly DomainErrorCode ProductNameTooLong =
        DomainErrorCode.Create("product_name_too_long");
}
