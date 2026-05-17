using Company.Template.Api.Options;
using Company.Template.Api.Routing;
using Company.Template.Api.Security;
using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Application.Products.ChangeProductPrice;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Application.Products.DiscontinueProduct;
using Company.Template.Application.Products.GetProductById;
using Company.Template.Application.Products.GetProducts;

namespace Company.Template.Api.Endpoints.Products;

/// <summary>
///     Defines the HTTP boundary for product workflows.
/// </summary>
/// <remarks>
///     Endpoints translate transport contracts into application commands and queries, delegate workflow coordination to
///     use cases, and map explicit application results back to HTTP responses.
/// </remarks>
internal sealed class ProductEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        AuthenticationOptions authenticationOptions = app.ServiceProvider
                                                         .GetRequiredService<IOptions<AuthenticationOptions>>()
                                                         .Value;

        RouteGroupBuilder group = app
                                 .MapGroup(ApiRoutes.Products.Base)
                                 .WithTags("Products");

        group
           .MapGet(ApiRoutes.Products.Collection, GetProductsAsync)
           .WithName(ApiRoutes.Products.Names.GetProducts)
           .Produces<PagedResponse<ProductResponse>>()
           .ProducesValidationProblem()
           .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
           .RequireTemplatePolicy(TemplatePolicies.ProductsRead, authenticationOptions.Enabled);

        group
           .MapPost(ApiRoutes.Products.Collection, CreateProductAsync)
           .WithName(ApiRoutes.Products.Names.CreateProduct)
           .Produces<ProductResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
           .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions.Enabled);

        group
           .MapGet(ApiRoutes.Products.ById, GetProductByIdAsync)
           .WithName(ApiRoutes.Products.Names.GetProductById)
           .Produces<ProductResponse>()
           .ProducesProblem(StatusCodes.Status404NotFound)
           .RequireTemplatePolicy(TemplatePolicies.ProductsRead, authenticationOptions.Enabled);

        group
           .MapPut(ApiRoutes.Products.Price, ChangeProductPriceAsync)
           .WithName(ApiRoutes.Products.Names.ChangeProductPrice)
           .Produces<ProductResponse>()
           .ProducesValidationProblem()
           .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status409Conflict)
           .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions.Enabled);

        group
           .MapPost(ApiRoutes.Products.Discontinue, DiscontinueProductAsync)
           .WithName(ApiRoutes.Products.Names.DiscontinueProduct)
           .Produces(StatusCodes.Status204NoContent)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status409Conflict)
           .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions.Enabled);
    }

    private static Task<IResult> CreateProductAsync(
        CreateProductRequest request,
        IUseCase<CreateProductCommand, ProductDto> useCase,
        CancellationToken cancellationToken)
    {
        return request
              .ToCommand()
              .BindAsync(command => useCase.ExecuteAsync(command, cancellationToken))
              .ToHttpResultAsync(product =>
                   Results.Created(
                       ApiRoutes.Products.Location(product.Id),
                       ProductEndpointMapper.ToResponse(product)));
    }

    private static async Task<IResult> GetProductByIdAsync(
        Guid productId,
        IUseCase<GetProductByIdQuery, ProductDto> useCase,
        CancellationToken cancellationToken)
    {
        Result<ProductDto> result = await useCase.ExecuteAsync(new GetProductByIdQuery(productId), cancellationToken);

        return result.ToHttpResult(product => Results.Ok(ProductEndpointMapper.ToResponse(product)));
    }

    private static Task<IResult> GetProductsAsync(
        [AsParameters] GetProductsRequest request,
        IUseCase<GetProductsQuery, PagedResult<ProductDto>> useCase,
        CancellationToken cancellationToken)
    {
        return request
              .ToQuery()
              .BindAsync(query => useCase.ExecuteAsync(query, cancellationToken))
              .ToHttpResultAsync(ProductEndpointMapper.ToResponse);
    }

    private static Task<IResult> ChangeProductPriceAsync(
        Guid productId,
        ChangeProductPriceRequest request,
        IUseCase<ChangeProductPriceCommand, ProductDto> useCase,
        CancellationToken cancellationToken)
    {
        return request
              .ToCommand(productId)
              .BindAsync(command => useCase.ExecuteAsync(command, cancellationToken))
              .ToHttpResultAsync(product => Results.Ok(ProductEndpointMapper.ToResponse(product)));
    }

    private static async Task<IResult> DiscontinueProductAsync(
        Guid productId,
        IUseCase<DiscontinueProductCommand> useCase,
        CancellationToken cancellationToken)
    {
        Result result = await useCase.ExecuteAsync(new DiscontinueProductCommand(productId), cancellationToken);

        return result.ToHttpResult();
    }
}
