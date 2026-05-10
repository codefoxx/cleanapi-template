using Company.Template.Api.Options;
using Microsoft.AspNetCore.OpenApi;

namespace Company.Template.Api.OpenApi;

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
