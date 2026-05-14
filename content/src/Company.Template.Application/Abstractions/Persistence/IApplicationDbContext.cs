namespace Company.Template.Application.Abstractions.Persistence;

/// <summary>
///     Defines the abstraction for the application's unit of work and data access.
/// </summary>
/// <remarks>
///     This interface keeps application code independent from the concrete DbContext
///     while exposing an EF Core-shaped unit of work. The actual implementation is
///     typically provided by EF Core.
/// </remarks>
public interface IApplicationDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>
    ///     Persists all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
