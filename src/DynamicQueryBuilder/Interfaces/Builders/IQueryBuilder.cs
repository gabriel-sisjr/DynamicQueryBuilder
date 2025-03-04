using DynamicQueryBuilder.Models.Enums.SqlOperators;

namespace DynamicQueryBuilder.Interfaces.Builders;

/// <summary>
///     Defines a builder for constructing SQL queries.
/// </summary>
public interface IQueryBuilder
{
    /// <summary>
    ///     Specifies the table from which to select data.
    /// </summary>
    /// <param name="table">The name of the table.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder FromTable(string table);

    /// <summary>
    ///     Specifies the columns to select in the query.
    /// </summary>
    /// <param name="columns">An array of column names.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder Columns(params string[] columns);

    /// <summary>
    ///     Adds a JOIN clause to the query.
    /// </summary>
    /// <param name="type">The type of join represented by a <see cref="JoinOperators" /> enum.</param>
    /// <param name="table">The table to join.</param>
    /// <param name="condition">The condition on which to join.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder Join(JoinOperators type, string table, string condition);

    /// <summary>
    ///     Adds a filter condition to the query using a raw string operator with AND.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The operator as a string.</param>
    /// <param name="value">The value for the filter.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder FilterBy(string column, string @operator, string value);

    /// <summary>
    ///     Adds a filter condition to the query using a comparison operator with AND.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The comparison operator.</param>
    /// <param name="value">The value for the filter.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder FilterBy(string column, ComparisonOperators @operator, string value);

    /// <summary>
    ///     Adds a filter condition for inclusion operators (IN, NOT IN) with AND.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The inclusion operator.</param>
    /// <param name="values">The values for the filter.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder FilterBy(string column, InclusionOperators @operator, IEnumerable<string> values);

    /// <summary>
    ///     Adds a filter condition for null checks with AND.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The null operator.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder FilterBy(string column, NullOperators @operator);

    /// <summary>
    ///     Adds a filter condition for range checks (BETWEEN, NOT BETWEEN) with AND.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The range operator.</param>
    /// <param name="start">The start value of the range.</param>
    /// <param name="end">The end value of the range.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder FilterBy(string column, RangeOperators @operator, string start, string end);

    /// <summary>
    ///     Adds a filter condition using a string operator (LIKE, ILIKE) with AND.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The string operator for filtering.</param>
    /// <param name="pattern">The pattern to match.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder FilterBy(string column, StringOperators @operator, string pattern);

    /// <summary>
    ///     Adds a filter condition to the query using a raw string operator with OR.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The operator as a string.</param>
    /// <param name="value">The value for the filter.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder OrFilterBy(string column, string @operator, string value);

    /// <summary>
    ///     Adds a filter condition to the query using a comparison operator with OR.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The comparison operator.</param>
    /// <param name="value">The value for the filter.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder OrFilterBy(string column, ComparisonOperators @operator, string value);

    /// <summary>
    ///     Adds a filter condition for inclusion operators (IN, NOT IN) with OR.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The inclusion operator.</param>
    /// <param name="values">The values for the filter.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder OrFilterBy(string column, InclusionOperators @operator, IEnumerable<string> values);

    /// <summary>
    ///     Adds a filter condition for null checks with OR.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The null operator.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder OrFilterBy(string column, NullOperators @operator);

    /// <summary>
    ///     Adds a filter condition for range checks (BETWEEN, NOT BETWEEN) with OR.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The range operator.</param>
    /// <param name="start">The start value of the range.</param>
    /// <param name="end">The end value of the range.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder OrFilterBy(string column, RangeOperators @operator, string start, string end);

    /// <summary>
    ///     Adds a filter condition using a string operator (LIKE, ILIKE) with OR.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The string operator for filtering.</param>
    /// <param name="pattern">The pattern to match.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder OrFilterBy(string column, StringOperators @operator, string pattern);

    /// <summary>
    ///     Specifies the GROUP BY clause for the query.
    /// </summary>
    /// <param name="columns">An array of column names to group by.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder GroupBy(params string[] columns);

    /// <summary>
    ///     Specifies the ORDER BY clause for the query using default ordering.
    /// </summary>
    /// <param name="columns">An array of column names to order by.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder OrderBy(params string[] columns);

    /// <summary>
    ///     Specifies the ORDER BY clause for a single column with a specified direction.
    /// </summary>
    /// <param name="column">The column name to order by.</param>
    /// <param name="direction">The direction of ordering.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder OrderBy(string column, OrderDirection direction);

    /// <summary>
    ///     Limits the number of records returned by the query.
    /// </summary>
    /// <param name="limit">The maximum number of records to return.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder SetLimit(int limit);

    /// <summary>
    ///     Generates the complete SQL query string based on the specified clauses.
    /// </summary>
    /// <returns>A SQL query string.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the table is undefined or not present in the provided database information.
    /// </exception>
    string GenerateSql();

    /// <summary>
    ///     Generates a formatted SQL query with proper line breaks and indentation.
    /// </summary>
    /// <returns>A SQL query string.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the table is undefined or not present in the provided database information.
    /// </exception>
    string GenerateFormattedSql();
}
