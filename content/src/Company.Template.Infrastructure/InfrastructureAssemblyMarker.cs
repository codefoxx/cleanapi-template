using System.Diagnostics.CodeAnalysis;

namespace Company.Template.Infrastructure;

/// <summary>
///     Marker type used by integration tests to locate the API assembly.
/// </summary>
[SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "The type is intentionally public so integration tests can reference the API assembly without exposing Program.")]
public sealed class InfrastructureAssemblyMarker;
