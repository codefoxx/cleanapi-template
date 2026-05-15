using Company.Template.Api.Tests.Products.Contracts;

namespace Company.Template.Api.Tests.Products;

internal static class ProductTestExtensions
{
    extension(HttpClient httpClient)
    {
        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest? request = null)
        {
            HttpResponseMessage response = await httpClient.SendJsonAsync(
                ProductEndpoints.Create,
                request ?? CreateProductRequest.Valid());

            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            ProductResponse product = await response.ReadJsonAsync<ProductResponse>();

            product.Id.ShouldNotBe(Guid.Empty);

            return product;
        }

        public async Task<ProductResponse> CreateDiscontinuedProductAsync()
        {
            ProductResponse product = await httpClient.CreateProductAsync();

            HttpResponseMessage response = await httpClient.SendAsync(
                ProductEndpoints.Discontinue(product.Id));

            response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

            HttpResponseMessage getResponse = await httpClient.SendAsync(
                ProductEndpoints.ById(product.Id));

            getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            ProductResponse discontinuedProduct = await getResponse.ReadJsonAsync<ProductResponse>();

            discontinuedProduct.Status.ShouldBe("Discontinued");
            discontinuedProduct.DiscontinuedAt.ShouldNotBeNull();

            return discontinuedProduct;
        }
    }
}
