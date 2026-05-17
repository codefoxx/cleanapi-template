// ReSharper disable once CheckNamespace
namespace Company.Template.Api.Routing;

internal static partial class ApiRoutes
{
    public static class Products
    {
        public const string Group = $"{Root}/products";

        public const string Collection = "";
        public const string ById = "/{productId:guid}";
        public const string Price = "/{productId:guid}/price";
        public const string Discontinue = "/{productId:guid}/discontinue";

        public static class Names
        {
            public const string GetProducts = "GetProducts";
            public const string CreateProduct = "CreateProduct";
            public const string GetProductById = "GetProductById";
            public const string ChangeProductPrice = "ChangeProductPrice";
            public const string DiscontinueProduct = "DiscontinueProduct";
        }
    }
}
