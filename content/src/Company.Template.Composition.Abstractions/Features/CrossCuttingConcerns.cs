namespace Company.Template.Composition.Abstractions.Features;

/// <summary>
///     Identifies shared technical concerns that cut across application features.
/// </summary>
/// <remarks>
///     This marker is reserved for concerns such as exception handling, problem details, request context access,
///     and use case telemetry. Security, persistence, and feature-specific behavior should be activated separately.
/// </remarks>
public sealed class CrossCuttingConcerns : IFeature;
