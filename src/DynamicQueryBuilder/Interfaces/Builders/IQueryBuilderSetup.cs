namespace DynamicQueryBuilder.Interfaces.Builders;

/// <summary>
///     Provides the initial setup functionality for the dynamic query builder.
///     <see cref="IQueryBuilder.GenerateSql" /> and <see cref="IQueryBuilder.GenerateFormattedSql" /> require
///     <see cref="Setup" /> to have been called first.
/// </summary>
/// <remarks>
///     This is the type returned by <c>QueryBuilder.Create()</c> and registered by <c>AddDynamicQueryBuilder</c>.
///     It does not stop the query methods from being called before <see cref="Setup" />: <c>QueryBuilder</c> also
///     implements <see cref="IQueryBuilder" /> publicly, and only the two generate methods check that setup happened.
/// </remarks>
public interface IQueryBuilderSetup
{
    /// <summary>
    ///     Configures the query builder with the required database metadata.
    ///     Call it before <see cref="IQueryBuilder.GenerateSql" /> or <see cref="IQueryBuilder.GenerateFormattedSql" />,
    ///     which throw <see cref="InvalidOperationException" /> otherwise. Calling it again replaces the dictionary.
    /// </summary>
    /// <param name="databaseInfo">
    ///     A dictionary where each key is a table name (as a string) and the associated value is
    ///     a list of column names (as strings) for that table.
    ///     Only the keys are used: the table given to <see cref="IQueryBuilder.FromTable" />, converted to upper case,
    ///     must be one of them, compared with the dictionary's own key comparer (case-sensitive by default).
    ///     The column lists are stored but never read, so columns are not validated.
    /// </param>
    /// <returns>
    ///     The same builder instance, as an <see cref="IQueryBuilder" /> that provides the full set of query-building methods.
    /// </returns>
    IQueryBuilder Setup(Dictionary<string, List<string>> databaseInfo);
}
