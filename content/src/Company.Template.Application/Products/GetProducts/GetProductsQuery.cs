namespace Company.Template.Application.Products.GetProducts;

public sealed record GetProductsQuery(PageRequest Page, ProductFilter Filter, ProductSort Sort);
