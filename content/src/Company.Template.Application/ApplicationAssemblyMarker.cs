using System.Diagnostics.CodeAnalysis;

namespace Company.Template.Application;

/// <summary>
///     Marker type used by the composition root to locate the Application assembly.
/// </summary>
[SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "The type is intentionally public so the composition root can reference the Application assembly.")]
public sealed class ApplicationAssemblyMarker;
