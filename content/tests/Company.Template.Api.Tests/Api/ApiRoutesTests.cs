using Company.Template.Api.Routing;

namespace Company.Template.Api.Tests.Api;

public sealed class ApiRoutesTests
{
    [Fact]
    public void ProductsRoutes_ShouldExposeExpectedRouteTemplates()
    {
        // Arrange

        // Act
        string root = ApiRoutes.Root;
        string productsBase = ApiRoutes.Products.Base;
        string collection = ApiRoutes.Products.Collection;
        string byId = ApiRoutes.Products.ById;
        string price = ApiRoutes.Products.Price;
        string discontinue = ApiRoutes.Products.Discontinue;

        // Assert
        root.ShouldBe("/api");
        productsBase.ShouldBe("/api/products");
        collection.ShouldBe("");
        byId.ShouldBe("/{productId:guid}");
        price.ShouldBe("/{productId:guid}/price");
        discontinue.ShouldBe("/{productId:guid}/discontinue");
    }

    [Fact]
    public void ProductsRouteNames_ShouldExposeExpectedNames()
    {
        // Arrange

        // Act
        string getProducts = ApiRoutes.Products.Names.GetProducts;
        string createProduct = ApiRoutes.Products.Names.CreateProduct;
        string getProductById = ApiRoutes.Products.Names.GetProductById;
        string changeProductPrice = ApiRoutes.Products.Names.ChangeProductPrice;
        string discontinueProduct = ApiRoutes.Products.Names.DiscontinueProduct;

        // Assert
        getProducts.ShouldBe("GetProducts");
        createProduct.ShouldBe("CreateProduct");
        getProductById.ShouldBe("GetProductById");
        changeProductPrice.ShouldBe("ChangeProductPrice");
        discontinueProduct.ShouldBe("DiscontinueProduct");
    }
}
