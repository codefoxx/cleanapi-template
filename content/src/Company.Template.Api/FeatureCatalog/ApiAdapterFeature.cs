namespace Company.Template.Api.FeatureCatalog;

/// <summary>
///     Identifies the HTTP API adapter boundary.
/// </summary>
/// <remarks>
///     The API adapter owns authentication, authorization, current-user access,
///     and the root health/info endpoint exposed by the API host.
/// </remarks>
public sealed class ApiAdapterFeature : IFeature;
