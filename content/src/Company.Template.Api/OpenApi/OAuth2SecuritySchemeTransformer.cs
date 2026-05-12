using Company.Template.Api.Options;
using Microsoft.AspNetCore.OpenApi;

namespace Company.Template.Api.OpenApi;

/// <summary>
/// Adds the configured OAuth2 client-credentials flow to generated OpenAPI documents.
/// </summary>
/// <remarks>
/// The transformer is intentionally conditional on authentication being enabled and an authority being configured, so
/// local or unauthenticated template runs do not advertise an identity-provider flow they cannot use.
/// </remarks>
internal sealed class OAuth2SecuritySchemeTransformer(
    IOptions<AuthenticationOptions> authenticationOptions)
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        AuthenticationOptions options = authenticationOptions.Value;

        if (!options.Enabled || string.IsNullOrWhiteSpace(options.Authority))
        {
            return Task.CompletedTask;
        }

        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);

        document.Components.SecuritySchemes["OAuth2"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "OAuth2 client credentials flow using the configured Keycloak realm.",
            Flows = new OpenApiOAuthFlows
            {
                ClientCredentials = new OpenApiOAuthFlow
                {
                    TokenUrl = new Uri($"{options.Authority}/protocol/openid-connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        ["products.read"] = "Allows reading product data.",
                        ["products.write"] = "Allows creating and modifying product data."
                    }
                }
            }
        };

        return Task.CompletedTask;
    }
}
