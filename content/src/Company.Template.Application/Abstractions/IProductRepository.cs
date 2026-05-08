using Company.Template.Domain.Products;

namespace Company.Template.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);
}
