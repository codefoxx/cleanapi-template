using Company.Template.Application.Abstractions.Persistence;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Infrastructure.Persistence;

public sealed partial class ApplicationDbContext : IRepository<Product, ProductId>
{
    public DbSet<Product> Products => Set<Product>();

    async Task<Option<Product>> IRepository<Product, ProductId>.FindAsync(
        ProductId key,
        CancellationToken cancellationToken)
    {
        Product? product = await Products.SingleOrDefaultAsync(product => product.Id == key, cancellationToken);

        return Option.FromNullable(product);
    }

    void IRepository<Product, ProductId>.Add(Product aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        Products.Add(aggregate);
    }

    void IRepository<Product, ProductId>.Delete(Product aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        Products.Remove(aggregate);
    }
}
