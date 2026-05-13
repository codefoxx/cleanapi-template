using Microsoft.AspNetCore.OpenApi;

namespace Company.Template.Api.OpenApi;

/// <summary>
///     Adds a bearer-token security scheme to generated OpenAPI documents.
/// </summary>
/// <remarks>
///     This keeps interactive documentation aligned with the API authentication boundary without changing runtime
///     authorization policy evaluation.
/// </remarks>
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter a JWT bearer token."
        };

        return Task.CompletedTask;
    }
}
