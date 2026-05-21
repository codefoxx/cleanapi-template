using Company.Template.Composition.Abstractions.Contracts;

namespace Company.Template.Composition.Abstractions.FeatureCatalog;

/// <summary>
///     Identifies shared technical concerns that cut across application features.
/// </summary>
/// <remarks>
///     This marker is reserved for concerns such as exception handling, problem details, and request context access.
///     Security, persistence, telemetry, and feature-specific behavior should be activated separately.
/// </remarks>
public sealed class CrossCuttingConcerns : IFeature;
