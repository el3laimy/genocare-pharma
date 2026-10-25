using GenoCare.Infrastructure.Persistence;
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
        services.AddScoped<InventoryPersistence>();
        services.AddScoped<DatabaseInitializer>();
        return services;
    }
}
