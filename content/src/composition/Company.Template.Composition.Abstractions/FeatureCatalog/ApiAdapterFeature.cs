using Company.Template.Composition.Abstractions.Contracts;

namespace Company.Template.Composition.Abstractions.FeatureCatalog;

/// <summary>
///     Identifies the HTTP API adapter boundary.
/// </summary>
/// <remarks>
///     The API adapter owns authentication, authorization, current-user access,
///     and the root health/info endpoint exposed by the API host.
/// </remarks>
public sealed class ApiAdapterFeature : IFeature;
