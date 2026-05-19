using Company.Template.Composition.Framework;

namespace Company.Template.Composition.Features;

/// <summary>
///     Identifies shared technical concerns that cut across application features.
/// </summary>
/// <remarks>
///     This marker is reserved for concerns such as exception handling, problem details, and request context access.
///     Security, persistence, telemetry, and feature-specific behavior should be activated separately.
/// </remarks>
public sealed class CrossCuttingConcerns : IFeature;
