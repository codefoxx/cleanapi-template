using Company.Template.Api.Routing;

namespace Company.Template.Api.Tests.Api;

public sealed class ApiRoutesTests
{
    [Fact]
    public void ProductsRoutes_ShouldExposeExpectedRouteTemplates()
    {
        ApiRoutes.Root.ShouldBe("/api");
        ApiRoutes.Products.Base.ShouldBe("/api/products");
        ApiRoutes.Products.Collection.ShouldBe("");
        ApiRoutes.Products.ById.ShouldBe("/{productId:guid}");
        ApiRoutes.Products.Price.ShouldBe("/{productId:guid}/price");
        ApiRoutes.Products.Discontinue.ShouldBe("/{productId:guid}/discontinue");
    }

    [Fact]
    public void ProductsRouteNames_ShouldExposeExpectedNames()
    {
        ApiRoutes.Products.Names.GetProducts.ShouldBe("GetProducts");
        ApiRoutes.Products.Names.CreateProduct.ShouldBe("CreateProduct");
        ApiRoutes.Products.Names.GetProductById.ShouldBe("GetProductById");
        ApiRoutes.Products.Names.ChangeProductPrice.ShouldBe("ChangeProductPrice");
        ApiRoutes.Products.Names.DiscontinueProduct.ShouldBe("DiscontinueProduct");
    }
}
