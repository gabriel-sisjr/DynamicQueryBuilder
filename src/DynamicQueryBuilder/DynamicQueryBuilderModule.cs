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
    /// <param name="settings">
    ///     A <see cref="DynamicQueryBuilderSettings"/> object containing the connection string and database driver definition.
    /// </param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    ///     Registers two scoped services: <see cref="IGetDatabaseMetaData"/>, created with <paramref name="settings"/>,
    ///     and <see cref="IQueryBuilderSetup"/>, implemented by <see cref="QueryBuilder"/>.
    ///     <see cref="IQueryBuilder"/> is not registered; call <see cref="IQueryBuilderSetup.Setup"/> to get one.
    ///     <paramref name="settings"/> is not validated here: an empty connection string only fails when
    ///     <see cref="IGetDatabaseMetaData.GetDatabaseTablesMetaDataAsync"/> is called.
    ///     Known issue: <see cref="QueryBuilder"/> keeps the clauses of the query being built, and the scoped
    ///     registration gives every consumer in the same scope (for example, the same HTTP request) the same
    ///     instance, so their clauses are mixed.
    /// </remarks>
    /// <example>
    ///     <code>
    ///     using DynamicQueryBuilder;
    ///     using DynamicQueryBuilder.Models.Enums.Helpers.Databases;
    ///     using DynamicQueryBuilder.Models.Helpers;
    ///
    ///     var builder = WebApplication.CreateBuilder(args);
    ///
    ///     var settings = new DynamicQueryBuilderSettings(
    ///         builder.Configuration.GetConnectionString("DynamicQueryBuilder")
    ///             ?? throw new InvalidOperationException("Connection string 'DynamicQueryBuilder' is missing."),
    ///         DatabaseDriver.POSTGRESQL);
    ///
    ///     builder.Services.AddDynamicQueryBuilder(settings);
    ///     </code>
    /// </example>
    public static IServiceCollection AddDynamicQueryBuilder(this IServiceCollection services,
        DynamicQueryBuilderSettings settings)
    {
        services.AddScoped<IGetDatabaseMetaData>(_ => new GetDatabaseMetaData(settings));
        services.AddScoped<IQueryBuilderSetup, QueryBuilder>();
        return services;
    }
}
