using Company.Template.Application.Abstractions;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products;

public interface IProductDbContext :IApplicationDbContext
{
    DbSet<Product> Products { get; }
    IQueryable<Product> ProductsForRead { get; }
}
