namespace DynamicQueryBuilder.Interfaces.Builders;

/// <summary>
///     Provides the initial setup functionality for the dynamic query builder.
///     This interface enforces that the necessary database metadata is provided
///     before any query construction methods become available.
/// </summary>
public interface IQueryBuilderSetup
{
    /// <summary>
    ///     Configures the query builder with the required database metadata.
    ///     This method must be called before proceeding with any query building operations.
    /// </summary>
    /// <param name="databaseInfo">
    ///     A dictionary where each key is a table name (as a string) and the associated value is
    ///     a list of column names (as strings) for that table.
    /// </param>
    /// <returns>
    ///     An instance of <see cref="IQueryBuilder" /> that provides the full set of query-building methods.
    /// </returns>
    IQueryBuilder Setup(Dictionary<string, List<string>> databaseInfo);
}
