// ReSharper disable once CheckNamespace

namespace Company.Template.Api.Routing;

internal static partial class ApiRoutes
{
    public static class Products
    {
        public const string Base = $"{Root}/products";

        public const string ById = "/{productId:guid}";
        public const string Collection = "";
        public const string Discontinue = "/{productId:guid}/discontinue";
        public const string Price = "/{productId:guid}/price";

        public static string Location(Guid productId)
        {
            return $"{Base}/{productId}";
        }

        public static class Names
        {
            public const string ChangeProductPrice = "ChangeProductPrice";
            public const string CreateProduct = "CreateProduct";
            public const string DiscontinueProduct = "DiscontinueProduct";
            public const string GetProductById = "GetProductById";
            public const string GetProducts = "GetProducts";
        }
    }
}
