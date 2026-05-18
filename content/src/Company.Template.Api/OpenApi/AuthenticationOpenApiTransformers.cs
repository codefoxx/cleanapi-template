using Company.Template.Api.Options;
using Microsoft.AspNetCore.OpenApi;

namespace Company.Template.Api.OpenApi;

internal static class AuthenticationOpenApiTransformers
{
    public static Task TransformDocumentAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        AuthenticationOptions authenticationOptions = context.ApplicationServices
                                                             .GetRequiredService<IOptions<AuthenticationOptions>>()
                                                             .Value;

        if (!authenticationOptions.Enabled)
        {
            return Task.CompletedTask;
        }

        OAuth2SecuritySchemeTransformer transformer = context.ApplicationServices
                                                            .GetRequiredService<OAuth2SecuritySchemeTransformer>();

        return transformer.TransformAsync(document, context, cancellationToken);
    }

    public static Task TransformOperationAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        AuthenticationOptions authenticationOptions = context.ApplicationServices
                                                             .GetRequiredService<IOptions<AuthenticationOptions>>()
                                                             .Value;

        if (!authenticationOptions.Enabled)
        {
            return Task.CompletedTask;
        }

        return new AuthorizationOperationTransformer()
           .TransformAsync(operation, context, cancellationToken);
    }
}
