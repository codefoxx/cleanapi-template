// ReSharper disable once CheckNamespace

namespace Company.Template.Application.Common;

public static partial class ErrorCodes
{
    public static readonly ErrorCode DiscontinuedProductCannotBeChanged =
        ErrorCode.Create("discontinued_product_cannot_be_changed");

    public static readonly ErrorCode ProductIdRequired =
        ErrorCode.Create("product_id_required");

    public static readonly ErrorCode ProductNameRequired =
        ErrorCode.Create("product_name_required");

    public static readonly ErrorCode ProductNameTooLong =
        ErrorCode.Create("product_name_too_long");
}
