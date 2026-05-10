using Company.Template.Api.Options;
using Company.Template.Api.Security;
using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.ChangeProductPrice;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Application.Products.DiscontinueProduct;
using Company.Template.Application.Products.GetProductById;

namespace Company.Template.Api.Endpoints.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        AuthenticationOptions authenticationOptions = app.ServiceProvider
            .GetRequiredService<IOptions<AuthenticationOptions>>()
            .Value;

        RouteGroupBuilder group = app
            .MapGroup("/api/products")
            .WithTags("Products");

        group
            .MapPost("/", CreateProductAsync)
            .WithName("CreateProduct")
            .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions.Enabled);

        group
            .MapGet("/{id:guid}", GetProductByIdAsync)
            .WithName("GetProductById")
            .RequireTemplatePolicy(TemplatePolicies.ProductsRead, authenticationOptions.Enabled);

        group
            .MapPut("/{id:guid}/price", ChangeProductPriceAsync)
            .WithName("ChangeProductPrice")
            .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions.Enabled);

        group
            .MapPost("/{id:guid}/discontinue", DiscontinueProductAsync)
            .WithName("DiscontinueProduct")
            .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions.Enabled);

        return app;
    }

    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request,
        IUseCase<CreateProductCommand, ProductDto> useCase,
        CancellationToken cancellationToken)
    {
        Result<ProductDto> result = await useCase.ExecuteAsync(
            new CreateProductCommand(request.Name, request.Price, request.Currency),
            cancellationToken);

        return result.ToHttpResult(product =>
            Results.Created($"/api/products/{product.Id}", ProductEndpointMapper.ToResponse(product)));
    }

    private static async Task<IResult> GetProductByIdAsync(
        Guid id,
        IUseCase<GetProductByIdQuery, ProductDto> useCase,
        CancellationToken cancellationToken)
    {
        Result<ProductDto> result = await useCase.ExecuteAsync(new GetProductByIdQuery(id), cancellationToken);

        return result.ToHttpResult(product => Results.Ok(ProductEndpointMapper.ToResponse(product)));
    }

    private static async Task<IResult> ChangeProductPriceAsync(
        Guid id,
        ChangeProductPriceRequest request,
        IUseCase<ChangeProductPriceCommand, ProductDto> useCase,
        CancellationToken cancellationToken)
    {
        Result<ProductDto> result = await useCase.ExecuteAsync(
            new ChangeProductPriceCommand(id, request.Price, request.Currency),
            cancellationToken);

        return result.ToHttpResult(product => Results.Ok(ProductEndpointMapper.ToResponse(product)));
    }

    private static async Task<IResult> DiscontinueProductAsync(
        Guid id,
        IUseCase<DiscontinueProductCommand> useCase,
        CancellationToken cancellationToken)
    {
        Result result = await useCase.ExecuteAsync(new DiscontinueProductCommand(id), cancellationToken);

        return result.ToHttpResult();
    }
}
