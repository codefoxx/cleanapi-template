using Company.Template.Api.Options;
using Company.Template.Api.Security;
using Company.Template.Application.Products.ChangeProductPrice;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Application.Products.DiscontinueProduct;
using Company.Template.Application.Products.GetProductById;

namespace Company.Template.Api.Endpoints.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var authenticationOptions = app.ServiceProvider.GetRequiredService<AuthenticationOptions>();

        var group = app
            .MapGroup("/api/products")
            .WithTags("Products");

        group
            .MapPost("/", CreateProductAsync)
            .WithName("CreateProduct")
            .WithOpenApi()
            .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions);

        group
            .MapGet("/{id:guid}", GetProductByIdAsync)
            .WithName("GetProductById")
            .WithOpenApi()
            .RequireTemplatePolicy(TemplatePolicies.ProductsRead, authenticationOptions);

        group
            .MapPut("/{id:guid}/price", ChangeProductPriceAsync)
            .WithName("ChangeProductPrice")
            .WithOpenApi()
            .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions);

        group
            .MapPost("/{id:guid}/discontinue", DiscontinueProductAsync)
            .WithName("DiscontinueProduct")
            .WithOpenApi()
            .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions);

        return app;
    }

    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request,
        CreateProductUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(
            new CreateProductCommand(request.Name, request.Price, request.Currency),
            cancellationToken);

        return result.ToHttpResult(product =>
            Results.Created($"/api/products/{product.Id}", ProductEndpointMapper.ToResponse(product)));
    }

    private static async Task<IResult> GetProductByIdAsync(
        Guid id,
        GetProductByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new GetProductByIdQuery(id), cancellationToken);

        return result.ToHttpResult(product => Results.Ok(ProductEndpointMapper.ToResponse(product)));
    }

    private static async Task<IResult> ChangeProductPriceAsync(
        Guid id,
        ChangeProductPriceRequest request,
        ChangeProductPriceUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(
            new ChangeProductPriceCommand(id, request.Price, request.Currency),
            cancellationToken);

        return result.ToHttpResult(product => Results.Ok(ProductEndpointMapper.ToResponse(product)));
    }

    private static async Task<IResult> DiscontinueProductAsync(
        Guid id,
        DiscontinueProductUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new DiscontinueProductCommand(id), cancellationToken);

        return result.ToHttpResult();
    }
}
