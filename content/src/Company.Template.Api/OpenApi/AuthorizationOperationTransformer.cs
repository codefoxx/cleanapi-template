using Microsoft.AspNetCore.OpenApi;

namespace Company.Template.Api.OpenApi;

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

        operation.Responses.TryAdd("401", new OpenApiResponse
        {
            Description = "Unauthorized"
        });

        operation.Responses.TryAdd("403", new OpenApiResponse
        {
            Description = "Forbidden"
        });

        return Task.CompletedTask;
    }
}
