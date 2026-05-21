using Company.Template.Api.Middleware;

namespace Company.Template.Api;

/// <summary>
///     Registers API services that support shared HTTP cross-cutting behavior.
/// </summary>
/// <remarks>
///     Security, authorization, OpenAPI, and feature-specific endpoint registrations are activated separately.
/// </remarks>
public sealed class ApiCrossCuttingServiceModule : IFeatureServiceModule<ApiCrossCuttingFeature>
{
    public void Register(FeatureServiceContext context)
    {
        context.Services.AddProblemDetails();
        context.Services.AddExceptionHandler<GlobalExceptionHandler>();
        context.Services.AddHttpContextAccessor();
    }
}
