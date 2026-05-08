using Company.Template.Application.Products.ChangeProductPrice;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Application.Products.DiscontinueProduct;
using Company.Template.Application.Products.GetProductById;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateProductUseCase>();
        services.AddScoped<GetProductByIdUseCase>();
        services.AddScoped<ChangeProductPriceUseCase>();
        services.AddScoped<DiscontinueProductUseCase>();

        return services;
    }
}
