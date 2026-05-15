namespace Company.Template.Api.Tests.Products;

internal static class ProductJsonContracts
{
    public static readonly RequiredJsonPaths Product = new(
        "$.id",
        "$.name",
        "$.price.amount",
        "$.price.currency",
        "$.status",
        "$.createdAt",
        "$.discontinuedAt");

    public static readonly RequiredJsonPaths ProductPage = new(
        "$.items.id",
        "$.items.name",
        "$.items.price.amount",
        "$.items.price.currency",
        "$.items.status",
        "$.items.createdAt",
        "$.items.discontinuedAt",
        "$.page.pageNumber",
        "$.page.pageSize",
        "$.page.hasPreviousPage",
        "$.page.hasNextPage",
        "$.total.totalCount",
        "$.total.totalPages");
}
