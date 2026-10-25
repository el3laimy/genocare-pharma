using Microsoft.Extensions.DependencyInjection;

namespace GenoCare.Application;

public sealed record ApplicationMarker;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddGenoCareApplication(this IServiceCollection services) => services;
}
