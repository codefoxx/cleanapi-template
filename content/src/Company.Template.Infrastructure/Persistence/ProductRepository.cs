using Company.Template.Application.Abstractions;
using Company.Template.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Company.Template.Infrastructure.Persistence;

internal sealed class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProductRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken)
    {
        return _dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
    }
}
