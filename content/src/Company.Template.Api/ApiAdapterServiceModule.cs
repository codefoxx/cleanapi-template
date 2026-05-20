using Company.Template.Api.CurrentUser;
//#if (auth == "Keycloak")
using Company.Template.Api.Security;
//#endif
using Company.Template.Application.Abstractions.Security;

namespace Company.Template.Api;

/// <summary>
///     Registers services owned by the HTTP API adapter boundary.
/// </summary>
/// <remarks>
///     Cross-cutting HTTP concerns, OpenAPI, and feature-specific endpoint registrations are activated separately.
/// </remarks>
public sealed class ApiAdapterServiceModule : IFeatureServiceModule<ApiAdapterFeature>
{
    public void Register(FeatureServiceContext context)
    {
        context.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

        //#if (auth == "Keycloak")
        context.Services.AddTemplateAuthentication();
        context.Services.AddTemplateAuthorization();
        //#endif
    }
}