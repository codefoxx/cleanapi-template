using Company.Template.Composition.Abstractions.Contracts;

namespace Company.Template.Composition.Abstractions.FeatureCatalog;

/// <summary>
///     Identifies API documentation registration and endpoint mapping.
/// </summary>
/// <remarks>
///     OpenAPI is an HTTP adapter concern that can be activated independently from authentication, authorization, and
///     endpoint feature modules.
/// </remarks>
public sealed class OpenApiFeature : IFeature;
