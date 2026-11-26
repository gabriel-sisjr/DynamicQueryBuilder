using DynamicQueryBuilder.Models.Helpers;

namespace DynamicQueryBuilder.Interfaces.Helpers;

/// <summary>
///     Interface for getting database metadata.
/// </summary>
public interface IGetDatabaseMetaData
{
    /// <summary>
    ///     Get the metadata of the database tables.
    /// </summary>
    /// <remarks>
    ///     Each call opens a connection with the configured settings and runs the catalog query of
    ///     <see cref="DynamicQueryBuilder.Drivers.DriversFactory.GetMetadataQuery"/>; nothing is cached.
    ///     The schemas <c>pg_catalog</c> and <c>information_schema</c> (PostgreSQL) or <c>mysql</c>,
    ///     <c>information_schema</c> and <c>performance_schema</c> (MySQL) are excluded. Names are returned as
    ///     the database reports them. This schema, table and columns shape is not the table-to-column-names
    ///     dictionary that <see cref="DynamicQueryBuilder.Interfaces.Builders.IQueryBuilderSetup.Setup"/> expects.
    /// </remarks>
    /// <returns>
    ///     A dictionary keyed by schema name. Each value is keyed by table name and holds one
    ///     <see cref="DatabaseTablesMetaData"/> per row of the catalog query.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to create a command.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the connection string is null, empty or whitespace, or by the driver when it is malformed.
    /// </exception>
    /// <exception cref="NotSupportedException">Thrown when the configured driver is not supported.</exception>
    /// <exception cref="System.Data.Common.DbException">
    ///     Thrown, as the driver's subtype (<c>NpgsqlException</c> or <c>MySqlException</c>), when connecting or running
    ///     the catalog query fails. With <see cref="DynamicQueryBuilder.Models.Enums.Helpers.Databases.DatabaseDriver.MYSQL" />
    ///     the query currently always fails with a <c>MySqlException</c>.
    /// </exception>
    Task<Dictionary<string, Dictionary<string, List<DatabaseTablesMetaData>>>> GetDatabaseTablesMetaDataAsync();
}
