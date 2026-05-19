using Company.Template.Composition.Abstractions.Contracts;

namespace Company.Template.Composition.Abstractions.FeatureCatalog;

/// <summary>
///     Identifies the domain event dispatching adapter used by the application.
/// </summary>
/// <remarks>
///     The default implementation is intentionally lightweight. Real applications can replace or extend this feature
///     with an outbox or message-based domain-event processing mechanism.
/// </remarks>
public sealed class DomainEventsFeature : IFeature;
