namespace DynamicQueryBuilder.Models.Enums.Helpers.Databases;

/// <summary>
///     Database drivers for reading table metadata.
/// </summary>
/// <remarks>
///     The driver selects the connection type and the catalog query used to read metadata.
///     It does not affect the SQL produced by the query builder.
/// </remarks>
public enum DatabaseDriver
{
    /// <summary>
    ///     PostgreSQL driver (Npgsql).
    /// </summary>
    POSTGRESQL = 0,

    /// <summary>
    ///     MySQL driver (MySql.Data).
    ///     Known issue: reading metadata with this driver fails. The MySQL catalog query selects <c>TABLE_SCHEMA</c>,
    ///     <c>TABLE_NAME</c> and <c>COLUMN_NAME</c> without a table qualifier although both joined views have these
    ///     columns, so MySQL rejects it with a <c>MySqlException</c>
    ///     ("Column 'TABLE_SCHEMA' in field list is ambiguous").
    /// </summary>
    MYSQL = 1
}
