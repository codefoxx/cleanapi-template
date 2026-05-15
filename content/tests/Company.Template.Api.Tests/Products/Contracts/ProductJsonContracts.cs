namespace Company.Template.Api.Tests.Products.Contracts;

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
        "$.page.number",
        "$.page.size",
        "$.page.hasPrevious",
        "$.page.hasNext",
        "$.total.items",
        "$.total.pages");
}
