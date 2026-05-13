using Company.Template.Api.Options;
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
///     use
///     cases, and map explicit application results back to HTTP responses.
/// </remarks>
internal sealed class ProductEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        AuthenticationOptions authenticationOptions = app.ServiceProvider
                                                         .GetRequiredService<IOptions<AuthenticationOptions>>()
                                                         .Value;

        RouteGroupBuilder group = app
                                 .MapGroup("/api/products")
                                 .WithTags("Products");

        group
           .MapGet("/", GetProductsAsync)
           .WithName("GetProducts")
           .Produces<PagedResponse<ProductResponse>>(StatusCodes.Status200OK)
           .ProducesValidationProblem(StatusCodes.Status400BadRequest)
           .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
           .RequireTemplatePolicy(TemplatePolicies.ProductsRead, authenticationOptions.Enabled);

        group
           .MapPost("/", CreateProductAsync)
           .WithName("CreateProduct")
           .Produces<ProductResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem(StatusCodes.Status400BadRequest)
           .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
           .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions.Enabled);

        group
           .MapGet("/{id:guid}", GetProductByIdAsync)
           .WithName("GetProductById")
           .Produces<ProductResponse>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .RequireTemplatePolicy(TemplatePolicies.ProductsRead, authenticationOptions.Enabled);

        group
           .MapPut("/{id:guid}/price", ChangeProductPriceAsync)
           .WithName("ChangeProductPrice")
           .Produces<ProductResponse>(StatusCodes.Status200OK)
           .ProducesValidationProblem(StatusCodes.Status400BadRequest)
           .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status409Conflict)
           .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions.Enabled);

        group
           .MapPost("/{id:guid}/discontinue", DiscontinueProductAsync)
           .WithName("DiscontinueProduct")
           .Produces(StatusCodes.Status204NoContent)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status409Conflict)
           .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions.Enabled);

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

    private static async Task<IResult> GetProductsAsync(
        [AsParameters] GetProductsRequest request,
        IUseCase<GetProductsQuery, PagedResult<ProductDto>> useCase,
        CancellationToken cancellationToken)
    {
        Result<PageRequest> page = PageRequest.Create(request.PageNumber, request.PageSize);

        if (!page.IsSuccess)
        {
            return page.ToProblemResult();
        }

        Result<ProductFilter> filter = ProductFilter.Create(
            request.Search,
            request.Status,
            request.Currency);

        if (!filter.IsSuccess)
        {
            return filter.ToProblemResult();
        }

        Result<ProductSort> sort = ProductSort.Create(
            request.SortBy,
            request.SortDirection);

        if (!sort.IsSuccess)
        {
            return sort.ToProblemResult();
        }

        GetProductsQuery query = new(
            page.Value!,
            filter.Value!,
            sort.Value!);

        Result<PagedResult<ProductDto>> result = await useCase.ExecuteAsync(query, cancellationToken);

        return result.ToHttpResult(ProductEndpointMapper.ToResponse);
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
