namespace Company.Template.Infrastructure.FeatureCatalog;

/// <summary>
///     Identifies the persistence adapter used by application workflows.
/// </summary>
/// <remarks>
///     The default implementation uses EF Core. Real applications can replace or extend this feature when persistence
///     needs to move behind a different storage adapter.
/// </remarks>
public sealed class PersistenceFeature : IFeature;
