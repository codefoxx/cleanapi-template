using Company.Template.Application.Abstractions.Persistence;
using Company.Template.Application.Common;
using Company.Template.Application.Products;
using Company.Template.Domain.Products;

namespace Company.Template.Infrastructure.Persistence;

public sealed partial class ApplicationDbContext : IRepository<Product, ProductId>, IProductDbContext
{
    public DbSet<Product> Products => Set<Product>();

    public IQueryable<Product> ProductsForRead => Products.AsNoTracking();

    async Task<Option<Product>> IRepository<Product, ProductId>.TryFindAsync(
        ProductId key,
        CancellationToken cancellationToken)
    {
        Product? product = await Products.SingleOrDefaultAsync(product => product.Id == key, cancellationToken);

        return Option.FromNullable(product);
    }

    void IRepository<Product, ProductId>.Add(Product aggregate)
    {
        Products.Add(aggregate);
    }

    void IRepository<Product, ProductId>.Delete(Product aggregate)
    {
        Products.Remove(aggregate);
    }
}
