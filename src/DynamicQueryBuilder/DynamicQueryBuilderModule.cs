using DynamicQueryBuilder.Builders;
using DynamicQueryBuilder.Databases;
using DynamicQueryBuilder.Interfaces.Builders;
using DynamicQueryBuilder.Interfaces.Helpers;
using DynamicQueryBuilder.Models.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicQueryBuilder;

/// <summary>
///     Module for adding Dynamic Query Builder services.
/// </summary>
public static class DynamicQueryBuilderModule
{
    /// <summary>
    ///     Adds Dynamic Query Builder services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">Settings object containing Connection String and Driver definition.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDynamicQueryBuilder(this IServiceCollection services,
        DynamicQueryBuilderSettings settings)
    {
        services.AddScoped<IGetDatabaseMetaData>(_ => new GetDatabaseMetaData(settings));
        services.AddScoped<IQueryBuilderSetup, QueryBuilder>();
        return services;
    }
}
