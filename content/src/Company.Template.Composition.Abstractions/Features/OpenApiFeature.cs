using Company.Template.Composition.Framework;

namespace Company.Template.Composition.Features;

/// <summary>
///     Identifies API documentation registration and endpoint mapping.
/// </summary>
/// <remarks>
///     OpenAPI is an HTTP adapter concern that can be activated independently from authentication, authorization, and
///     endpoint feature modules.
/// </remarks>
public sealed class OpenApiFeature : IFeature;
