using System.Data;
using DynamicQueryBuilder.Models.Enums.Helpers.Databases;
using MySql.Data.MySqlClient;
using Npgsql;

namespace DynamicQueryBuilder.Drivers;

/// <summary>
///     Factory class for creating database connections and metadata queries.
/// </summary>
public static class DriversFactory
{
    /// <summary>
    ///     Get a database connection.
    /// </summary>
    /// <param name="driver">The database driver.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>
    ///     A new connection that is not opened yet: an <see cref="Npgsql.NpgsqlConnection"/> for
    ///     <see cref="DatabaseDriver.POSTGRESQL"/> or a <see cref="MySql.Data.MySqlClient.MySqlConnection"/> for
    ///     <see cref="DatabaseDriver.MYSQL"/>. The caller opens and disposes it.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> is null.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="connectionString"/> is empty or whitespace, or by the driver's connection
    ///     constructor when it is malformed.
    /// </exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="driver"/> is not supported.</exception>
    public static IDbConnection GetDatabaseConnection(DatabaseDriver driver, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));

        return driver switch
        {
            DatabaseDriver.POSTGRESQL => new NpgsqlConnection(connectionString),
            DatabaseDriver.MYSQL => new MySqlConnection(connectionString),
            _ => throw new NotSupportedException("UNSUPPORTED DATABASE DRIVER.")
        };
    }

    /// <summary>
    ///     Get the metadata query for a database driver.
    /// </summary>
    /// <param name="driver">The database driver.</param>
    /// <returns>
    ///     The SQL text of the catalog query. Its eight columns are read by position: schema, table, column,
    ///     data type, the value mapped to <see cref="DynamicQueryBuilder.Models.Helpers.DatabaseTablesMetaData.PrimaryKey"/>,
    ///     and the related schema, table and column of a foreign key.
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="driver"/> is not supported.</exception>
    public static string GetMetadataQuery(DatabaseDriver driver)
    {
        return driver switch
        {
            DatabaseDriver.POSTGRESQL => """
                                         SELECT
                                             table_schema, table_name, column_name, data_type,
                                             column_default AS primary_key,
                                             foreign_table_schema, foreign_table_name, foreign_column_name
                                         FROM information_schema.columns
                                         LEFT JOIN (
                                             SELECT
                                                 tc.table_schema, tc.table_name, kcu.column_name,
                                                 ccu.table_schema AS foreign_table_schema,
                                                 ccu.table_name AS foreign_table_name,
                                                 ccu.column_name AS foreign_column_name
                                             FROM information_schema.table_constraints AS tc
                                             JOIN information_schema.key_column_usage AS kcu
                                                 ON tc.constraint_name = kcu.constraint_name
                                                 AND tc.table_schema = kcu.table_schema
                                             JOIN information_schema.constraint_column_usage AS ccu
                                                 ON ccu.constraint_name = tc.constraint_name
                                             WHERE tc.constraint_type = 'FOREIGN KEY'
                                         ) AS fk_info
                                         USING (table_schema, table_name, column_name)
                                         WHERE table_schema NOT IN ('pg_catalog', 'information_schema');
                                         """,
            DatabaseDriver.MYSQL => """
                                        SELECT
                                            TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, COLUMN_TYPE,
                                            COLUMN_KEY AS primary_key,
                                            REFERENCED_TABLE_SCHEMA, REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME
                                        FROM INFORMATION_SCHEMA.COLUMNS
                                        LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                                            ON INFORMATION_SCHEMA.COLUMNS.TABLE_SCHEMA = INFORMATION_SCHEMA.KEY_COLUMN_USAGE.TABLE_SCHEMA
                                            AND INFORMATION_SCHEMA.COLUMNS.TABLE_NAME = INFORMATION_SCHEMA.KEY_COLUMN_USAGE.TABLE_NAME
                                            AND INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME = INFORMATION_SCHEMA.KEY_COLUMN_USAGE.COLUMN_NAME
                                        WHERE TABLE_SCHEMA NOT IN ('mysql', 'information_schema', 'performance_schema');
                                    """,
            _ => throw new NotSupportedException("UNSUPPORTED DATABASE DRIVER.")
        };
    }
}
