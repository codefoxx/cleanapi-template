namespace Company.Template.Application.Abstractions.Persistence;

/// <summary>
///     Marks an application query abstraction implemented by infrastructure.
/// </summary>
/// <remarks>
///     Query interfaces describe read operations needed by the application layer.
///     Infrastructure implementations may use EF Core, projections, joins, filtering,
///     sorting, and other persistence-specific optimizations.
/// </remarks>
public interface IQuery;
