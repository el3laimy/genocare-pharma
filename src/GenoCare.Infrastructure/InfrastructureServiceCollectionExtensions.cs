using GenoCare.Infrastructure.Persistence;
using GenoCare.Application.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenoCare.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddGenoCareInfrastructure(this IServiceCollection services, string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        services.AddPooledDbContextFactory<GenoCareDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));
        services.AddScoped<CatalogPersistence>();
        services.AddScoped<IProductCatalogStore>(sp => sp.GetRequiredService<CatalogPersistence>());
        services.AddScoped<IBatchCatalogStore>(sp => sp.GetRequiredService<CatalogPersistence>());
        services.AddScoped<InventoryPersistence>();
        services.AddScoped<IInventoryReadStore>(sp => sp.GetRequiredService<InventoryPersistence>());
        services.AddScoped<DatabaseInitializer>();
        return services;
    }
}
