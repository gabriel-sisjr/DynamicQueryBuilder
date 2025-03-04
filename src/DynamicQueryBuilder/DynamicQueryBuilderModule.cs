using DynamicQueryBuilder.Databases;
using DynamicQueryBuilder.Interfaces.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicQueryBuilder;

/// <summary>
/// Module for adding Dynamic Query Builder services.
/// </summary>
public static class DynamicQueryBuilderModule
{
    /// <summary>
    /// Adds Dynamic Query Builder services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDynamicQueryBuilder(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IGetDatabaseMetaData>(_ => new GetDatabaseMetaData(connectionString));
        return services;
    }
}
