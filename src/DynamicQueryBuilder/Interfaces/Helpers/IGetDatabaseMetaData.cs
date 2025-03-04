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
    /// <returns>A dictionary of schemas and tables with their metadata.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to create a command.</exception>
    Task<Dictionary<string, Dictionary<string, List<DatabaseTablesMetaData>>>> GetDatabaseTablesMetaDataAsync();
}
