using Company.Template.Domain.Products;

namespace Company.Template.Application.Products;

/// <summary>
///     Product-specific persistence boundary used by application use cases.
/// </summary>
/// <remarks>
///     The abstraction decouples application code from the concrete DbContext while remaining EF Core-shaped:
///     commands can track aggregates for changes, and queries can use a read-oriented product source.
/// </remarks>
public interface IProductDbContext : IApplicationDbContext
{
    DbSet<Product> Products { get; }
    IQueryable<Product> ProductsForRead { get; }
}
