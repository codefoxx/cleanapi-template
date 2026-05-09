using Company.Template.Domain.Products;

namespace Company.Template.Application.Products;

public static class ProductQueries
{
    extension(IQueryable<Product> products)
    {
        public IQueryable<Product> WithId(ProductId productId)
        {
            return products.Where(product => product.Id == productId);
        }

        public IQueryable<Product> Active()
        {
            return products.Where(product => product.Status == ProductStatus.Active);
        }
    }
}
