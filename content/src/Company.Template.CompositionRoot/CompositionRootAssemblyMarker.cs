using System.Diagnostics.CodeAnalysis;

namespace Company.Template.CompositionRoot;

/// <summary>
///     Marker type used by integration tests and tooling to locate the executable composition assembly.
/// </summary>
[SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "The type is intentionally public so integration tests can reference the application entry point.")]
public sealed class CompositionRootAssemblyMarker;
