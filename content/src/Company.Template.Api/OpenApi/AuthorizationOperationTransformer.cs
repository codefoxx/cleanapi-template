using Microsoft.AspNetCore.OpenApi;

namespace Company.Template.Api.OpenApi;

/// <summary>
///     Projects endpoint authorization metadata into OpenAPI operation security requirements.
/// </summary>
/// <remarks>
///     Runtime authorization remains enforced by ASP.NET Core policies; this transformer only documents the required
///     policies and possible authentication or authorization failures for consumers.
/// </remarks>
internal sealed class AuthorizationOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        IList<object> metadata = context.Description.ActionDescriptor.EndpointMetadata;

        if (metadata.OfType<IAllowAnonymous>().Any())
        {
            return Task.CompletedTask;
        }

        List<string> policies = metadata
                               .OfType<IAuthorizeData>()
                               .Select(authorizeData => authorizeData.Policy)
                               .Where(policy => !string.IsNullOrWhiteSpace(policy))
                               .Select(policy => policy!)
                               .Distinct(StringComparer.Ordinal)
                               .ToList();

        if (policies.Count == 0)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];

        OpenApiSecuritySchemeReference securityScheme = new("OAuth2", context.Document);

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [securityScheme] = policies
        });

        operation.Responses ??= new OpenApiResponses
        {
            Extensions = null
        };

        operation.Responses.TryAdd("401",
            new OpenApiResponse
            {
                Description = "Unauthorized"
            });

        operation.Responses.TryAdd("403",
            new OpenApiResponse
            {
                Description = "Forbidden"
            });

        return Task.CompletedTask;
    }
}
