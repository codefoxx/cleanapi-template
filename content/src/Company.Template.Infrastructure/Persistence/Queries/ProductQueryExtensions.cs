using Company.Template.Application.Common;
using Company.Template.Application.Products.GetProducts;
using Company.Template.Domain.Products;
using Company.Template.Domain.SharedKernel;

namespace Company.Template.Infrastructure.Persistence.Queries;

internal static class ProductQueryExtensions
{
    extension(IQueryable<Product> products)
    {
        public IQueryable<Product> Active()
        {
            return products.Where(product => product.Status == ProductStatus.Active);
        }

        public IQueryable<Product> WithDefaultVisibility(ProductFilter filter)
        {
            ArgumentNullException.ThrowIfNull(products);
            ArgumentNullException.ThrowIfNull(filter);

            return filter.Status.HasValue
                ? products
                : products.Active();
        }

        public IQueryable<Product> WithId(ProductId productId)
        {
            return products.Where(product => product.Id == productId);
        }

        public IQueryable<Product> WithSorting(ProductSort sort)
        {
            bool ascending = sort.Direction == SortDirection.Ascending;

            if (sort.Field == ProductSortField.Name)
            {
                return ascending
                    ? products.OrderBy(product => product.Name.Value)
                    : products.OrderByDescending(product => product.Name.Value);
            }

            if (sort.Field == ProductSortField.Price)
            {
                return ascending
                    ? products.OrderBy(product => product.Price.Amount)
                    : products.OrderByDescending(product => product.Price.Amount);
            }

            if (sort.Field == ProductSortField.Status)
            {
                return ascending
                    ? products.OrderBy(product => product.Status)
                    : products.OrderByDescending(product => product.Status);
            }

            return ascending
                ? products.OrderBy(product => product.CreatedAt)
                : products.OrderByDescending(product => product.CreatedAt);
        }

        public IQueryable<Product> WithFilter(ProductFilter filter)
        {
            ArgumentNullException.ThrowIfNull(products);
            ArgumentNullException.ThrowIfNull(filter);

            IQueryable<Product> query = products;

            if (filter.Search.TryGetValue(out string search))
            {
                query = query.Where(product => product.Name.Value.Contains(search));
            }

            if (filter.Status.TryGetValue(out ProductStatus status))
            {
                query = query.Where(product => product.Status == status);
            }

            if (filter.Currency.TryGetValue(out Currency currency))
            {
                query = query.Where(product => product.Price.Currency == currency);
            }

            return query;
        }
    }
}
