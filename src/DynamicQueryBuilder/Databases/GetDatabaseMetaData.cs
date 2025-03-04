using System.Data;
using System.Data.Common;
using DynamicQueryBuilder.Drivers;
using DynamicQueryBuilder.Interfaces.Helpers;
using DynamicQueryBuilder.Models.Enums.Helpers.Databases;
using DynamicQueryBuilder.Models.Helpers;

namespace DynamicQueryBuilder.Databases;

/// <summary>
/// Get the metadata of the database tables.
/// </summary>
public sealed class GetDatabaseMetaData(string connectionString) : IGetDatabaseMetaData
{
    /// <summary>
    /// Get the metadata of the database tables.
    /// </summary>
    /// <param name="driver">The database driver.</param>
    /// <returns>A dictionary of schemas and tables with their metadata.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to create a command.</exception>
    public async Task<Dictionary<string, Dictionary<string, List<DatabaseTablesMetaData>>>> GetDatabaseTablesMetaDataAsync(DatabaseDriver driver)
    {
        var schemaTablesDict = new Dictionary<string, Dictionary<string, List<DatabaseTablesMetaData>>>();

        using IDbConnection conn = DriversFactory.GetDatabaseConnection(driver, connectionString);
        if (conn is DbConnection dbConn)
            await dbConn.OpenAsync();
        else
            conn.Open();

        await using var cmd = conn.CreateCommand() as DbCommand;
        if (cmd == null)
            throw new InvalidOperationException("Unable to create command.");

        cmd.CommandText = DriversFactory.GetMetadataQuery(driver);

        await using DbDataReader reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var schema = reader.GetString(0);
            var table = reader.GetString(1);
            var column = reader.GetString(2);
            var columnType = reader.GetString(3);
            var primaryKey = reader.IsDBNull(4) ? null : reader.GetString(4);
            var relatedSchema = reader.IsDBNull(5) ? null : reader.GetString(5);
            var relatedTable = reader.IsDBNull(6) ? null : reader.GetString(6);
            var relatedColumn = reader.IsDBNull(7) ? null : reader.GetString(7);

            if (!schemaTablesDict.ContainsKey(schema))
                schemaTablesDict[schema] = new Dictionary<string, List<DatabaseTablesMetaData>>();

            if (!schemaTablesDict[schema].ContainsKey(table))
                schemaTablesDict[schema][table] = [];

            schemaTablesDict[schema][table].Add(new DatabaseTablesMetaData(
                column, columnType, primaryKey, relatedSchema, relatedTable, relatedColumn
            ));
        }

        return schemaTablesDict;
    }
}
