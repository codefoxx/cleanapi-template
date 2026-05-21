namespace Company.Template.Infrastructure.FeatureCatalog;

/// <summary>
///     Identifies infrastructure-level cross-cutting services.
/// </summary>
/// <remarks>
///     This marker is reserved for technical infrastructure services that support
///     application workflows without belonging to persistence or domain-event dispatching.
/// </remarks>
public sealed class InfrastructureCrossCuttingFeature : IFeature;
