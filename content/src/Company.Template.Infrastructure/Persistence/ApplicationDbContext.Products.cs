using Company.Template.Application.Products;
using Company.Template.Domain.Products;

namespace Company.Template.Infrastructure.Persistence;

public sealed partial class ApplicationDbContext : IProductDbContext
{
    public DbSet<Product> Products => Set<Product>();
    public IQueryable<Product> ProductsForRead => Products.AsNoTracking();
}
