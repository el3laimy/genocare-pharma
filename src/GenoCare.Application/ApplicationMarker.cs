using Microsoft.Extensions.DependencyInjection;
using GenoCare.Application.Catalog;

namespace GenoCare.Application;

public sealed record ApplicationMarker;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddGenoCareApplication(this IServiceCollection services)
    {
        services.AddScoped<ProductCatalogUseCases>();
        services.AddScoped<BatchCatalogUseCases>();
        return services;
    }
}
