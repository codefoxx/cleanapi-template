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
internal sealed class ProductEndpoints
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
        return request.ToCommand()
                      .Match(
                           success: command => ExecuteCreateProductAsync(useCase, command, cancellationToken),
                           failure: error => Task.FromResult(ProductResults.FromValidation(error)));
    }

    private static async Task<IResult> ExecuteCreateProductAsync(
        IUseCase<CreateProductCommand, ProductDto> useCase,
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        Result<ProductDto> result = await useCase.ExecuteAsync(command, cancellationToken);

        return result.Match(
            success: product => Results.Created(
                ApiRoutes.Products.Location(product.Id),
                ProductResponse.FromDto(product)),
            failure: ProductResults.FromError);
    }

    private static async Task<IResult> GetProductByIdAsync(
        Guid productId,
        IUseCase<GetProductByIdQuery, ProductDto> useCase,
        CancellationToken cancellationToken)
    {
        GetProductByIdQuery query = new(productId);

        Result<ProductDto> result = await useCase.ExecuteAsync(query, cancellationToken);

        return result.Match(
            success: product => Results.Ok(ProductResponse.FromDto(product)),
            failure: ProductResults.FromError);
    }

    private static Task<IResult> GetProductsAsync(
        [AsParameters] GetProductsRequest request,
        IUseCase<GetProductsQuery, PagedResult<ProductDto>> useCase,
        CancellationToken cancellationToken)
    {
        return request.ToQuery()
                      .Match(
                           success: query => ExecuteGetProductsAsync(useCase, query, cancellationToken),
                           failure: error => Task.FromResult(ProductResults.FromValidation(error)));
    }

    private static async Task<IResult> ExecuteGetProductsAsync(
        IUseCase<GetProductsQuery, PagedResult<ProductDto>> useCase,
        GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        Result<PagedResult<ProductDto>> result = await useCase.ExecuteAsync(query, cancellationToken);

        return result.Match(
            success: products => Results.Ok(PagedResponse<ProductResponse>.FromPagedResult(
                products,
                ProductResponse.FromDto)),
            failure: ProductResults.FromError);
    }

    private static Task<IResult> ChangeProductPriceAsync(
        Guid productId,
        ChangeProductPriceRequest request,
        IUseCase<ChangeProductPriceCommand, ProductDto> useCase,
        CancellationToken cancellationToken)
    {
        return request.ToCommand(productId)
                      .Match(
                           success: command => ExecuteChangeProductPriceAsync(useCase, command, cancellationToken),
                           failure: error => Task.FromResult(ProductResults.FromValidation(error)));
    }

    private static async Task<IResult> ExecuteChangeProductPriceAsync(
        IUseCase<ChangeProductPriceCommand, ProductDto> useCase,
        ChangeProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        Result<ProductDto> result = await useCase.ExecuteAsync(command, cancellationToken);

        return result.Match(
            success: product => Results.Ok(ProductResponse.FromDto(product)),
            failure: ProductResults.FromError);
    }

    private static async Task<IResult> DiscontinueProductAsync(
        Guid productId,
        IUseCase<DiscontinueProductCommand> useCase,
        CancellationToken cancellationToken)
    {
        DiscontinueProductCommand command = new(productId);

        Result result = await useCase.ExecuteAsync(command, cancellationToken);

        return result.Match(
            success: Results.NoContent,
            failure: ProductResults.FromError);
    }
}
