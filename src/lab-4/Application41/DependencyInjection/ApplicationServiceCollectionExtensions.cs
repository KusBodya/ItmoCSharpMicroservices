using Application41.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application41.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<OrderHistoryFactory>();
        services.AddScoped<OrderStateValidator>();

        return services;
    }
}