using System.Data;
using DynamicQueryBuilder.Models.Enums.Helpers.Databases;
using MySql.Data.MySqlClient;
using Npgsql;

namespace DynamicQueryBuilder.Drivers;

/// <summary>
/// Factory class for creating database connections and metadata queries.
/// </summary>
public static class DriversFactory
{
    /// <summary>
    /// Get a database connection.
    /// </summary>
    /// <param name="driver">The database driver.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>A database connection.</returns>
    public static IDbConnection GetDatabaseConnection(DatabaseDriver driver, string connectionString)
        => driver switch
        {
            DatabaseDriver.POSTGRESQL => new NpgsqlConnection(connectionString),
            DatabaseDriver.MYSQL => new MySqlConnection(connectionString),
            _ => throw new NotSupportedException("UNSUPPORTED DATABASE DRIVER.")
        };

    /// <summary>
    /// Get the metadata query for a database driver.
    /// </summary>
    /// <param name="driver">The database driver.</param>
    /// <returns>The metadata query.</returns>
    public static string GetMetadataQuery(DatabaseDriver driver)
        => driver switch
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
