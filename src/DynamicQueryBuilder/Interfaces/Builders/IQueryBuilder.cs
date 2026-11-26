using DynamicQueryBuilder.Models.Enums.SqlOperators;

namespace DynamicQueryBuilder.Interfaces.Builders;

/// <summary>
///     Defines a builder for constructing SQL queries.
/// </summary>
/// <remarks>
///     <para>
///         Column names, joined tables, join conditions, operator strings and values are inserted into the generated
///         SQL verbatim, and so is the table of <see cref="FromTable" />, except that it is converted to upper case.
///         Nothing is parameterized, quoted or escaped, and nothing is checked against the metadata given to
///         <see cref="IQueryBuilderSetup.Setup" /> except the table set by <see cref="FromTable" />.
///         Do not pass untrusted input to these methods: it allows SQL injection.
///         String literals must carry their own quotes, for example <c>"'Alice'"</c>.
///     </para>
///     <para>
///         Filters are emitted in the order they were added, joined by AND or OR without parentheses, so the
///         database's operator precedence applies. The AND or OR of the first filter is not emitted.
///     </para>
///     <para>
///         Each clause method changes this instance and returns it. Columns, joins, filters, GROUP BY and ORDER BY
///         items are only appended, and there is no way to remove them or reset the builder;
///         <see cref="FromTable" /> and <see cref="SetLimit" /> replace their value. The generate methods do not
///         change the instance.
///     </para>
/// </remarks>
public interface IQueryBuilder
{
    /// <summary>
    ///     Specifies the table from which to select data.
    /// </summary>
    /// <remarks>
    ///     The name is converted with <see cref="string.ToUpper()" />, which uses the current culture, and is
    ///     emitted without quotes. Calling it again replaces the table.
    /// </remarks>
    /// <param name="table">The name of the table.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder FromTable(string table);

    /// <summary>
    ///     Specifies the columns to select in the query.
    ///     When no column is specified, <c>SELECT *</c> is generated.
    /// </summary>
    /// <param name="columns">An array of column names, emitted verbatim and separated by commas.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder Columns(params string[] columns);

    /// <summary>
    ///     Adds a JOIN clause to the query, emitted as <c>{join type} {table} ON {condition}</c>.
    /// </summary>
    /// <remarks>
    ///     Known issues: <see cref="JoinOperators.CROSS_JOIN" /> is also emitted with <c>ON</c>, which standard SQL
    ///     does not allow for a cross join. <see cref="JoinOperators.SELF_JOIN" /> is emitted as <c>SELF JOIN</c>,
    ///     which is not an SQL join type: PostgreSQL and MySQL read <c>SELF</c> as an alias for the FROM table, so the
    ///     query fails when the condition names the FROM table and otherwise runs as a plain inner join.
    ///     <see cref="JoinOperators.FULL_JOIN" /> does not exist in MySQL, which reads the text as an alias plus a
    ///     plain join, or fails.
    /// </remarks>
    /// <param name="type">The type of join represented by a <see cref="JoinOperators" /> enum.</param>
    /// <param name="table">The table to join, emitted verbatim.</param>
    /// <param name="condition">The condition on which to join, emitted verbatim.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="type" /> is not a defined value.</exception>
    IQueryBuilder Join(JoinOperators type, string table, string condition);

    /// <summary>
    ///     Adds a filter condition to the query using a raw string operator with AND.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The operator as a string, emitted verbatim.</param>
    /// <param name="value">The value for the filter, emitted verbatim.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder FilterBy(string column, string @operator, string value);

    /// <summary>
    ///     Adds a filter condition to the query using a comparison operator with AND.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The comparison operator.</param>
    /// <param name="value">The value for the filter, emitted verbatim.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="operator" /> is not a defined value.</exception>
    IQueryBuilder FilterBy(string column, ComparisonOperators @operator, string value);

    /// <summary>
    ///     Adds a filter condition for inclusion operators (IN, NOT IN) with AND.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The inclusion operator.</param>
    /// <param name="values">
    ///     The values for the filter, emitted verbatim, separated by commas and enclosed in parentheses.
    ///     An empty sequence produces empty parentheses.
    /// </param>
    /// <returns>The current IQueryBuilder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="operator" /> is not a defined value.</exception>
    IQueryBuilder FilterBy(string column, InclusionOperators @operator, IEnumerable<string> values);

    /// <summary>
    ///     Adds a filter condition for null checks with AND.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The null operator.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="operator" /> is not a defined value.</exception>
    IQueryBuilder FilterBy(string column, NullOperators @operator);

    /// <summary>
    ///     Adds a filter condition for range checks (BETWEEN, NOT BETWEEN) with AND.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The range operator.</param>
    /// <param name="start">The start value of the range, emitted verbatim.</param>
    /// <param name="end">The end value of the range, emitted verbatim.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="operator" /> is not a defined value.</exception>
    IQueryBuilder FilterBy(string column, RangeOperators @operator, string start, string end);

    /// <summary>
    ///     Adds a filter condition using a string operator (LIKE, ILIKE) with AND.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The string operator for filtering.</param>
    /// <param name="pattern">The pattern to match, emitted verbatim, so it includes its quotes, for example <c>"'A%'"</c>.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="operator" /> is <see cref="StringOperators.CONCAT" /> or not a defined value.
    /// </exception>
    IQueryBuilder FilterBy(string column, StringOperators @operator, string pattern);

    /// <summary>
    ///     Adds a filter condition to the query using a raw string operator with OR.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The operator as a string, emitted verbatim.</param>
    /// <param name="value">The value for the filter, emitted verbatim.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder OrFilterBy(string column, string @operator, string value);

    /// <summary>
    ///     Adds a filter condition to the query using a comparison operator with OR.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The comparison operator.</param>
    /// <param name="value">The value for the filter, emitted verbatim.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="operator" /> is not a defined value.</exception>
    IQueryBuilder OrFilterBy(string column, ComparisonOperators @operator, string value);

    /// <summary>
    ///     Adds a filter condition for inclusion operators (IN, NOT IN) with OR.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The inclusion operator.</param>
    /// <param name="values">
    ///     The values for the filter, emitted verbatim, separated by commas and enclosed in parentheses.
    ///     An empty sequence produces empty parentheses.
    /// </param>
    /// <returns>The current IQueryBuilder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="operator" /> is not a defined value.</exception>
    IQueryBuilder OrFilterBy(string column, InclusionOperators @operator, IEnumerable<string> values);

    /// <summary>
    ///     Adds a filter condition for null checks with OR.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The null operator.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="operator" /> is not a defined value.</exception>
    IQueryBuilder OrFilterBy(string column, NullOperators @operator);

    /// <summary>
    ///     Adds a filter condition for range checks (BETWEEN, NOT BETWEEN) with OR.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The range operator.</param>
    /// <param name="start">The start value of the range, emitted verbatim.</param>
    /// <param name="end">The end value of the range, emitted verbatim.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="operator" /> is not a defined value.</exception>
    IQueryBuilder OrFilterBy(string column, RangeOperators @operator, string start, string end);

    /// <summary>
    ///     Adds a filter condition using a string operator (LIKE, ILIKE) with OR.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="operator">The string operator for filtering.</param>
    /// <param name="pattern">The pattern to match, emitted verbatim, so it includes its quotes, for example <c>"'A%'"</c>.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="operator" /> is <see cref="StringOperators.CONCAT" /> or not a defined value.
    /// </exception>
    IQueryBuilder OrFilterBy(string column, StringOperators @operator, string pattern);

    /// <summary>
    ///     Specifies the GROUP BY clause for the query.
    /// </summary>
    /// <param name="columns">An array of column names to group by, emitted verbatim.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder GroupBy(params string[] columns);

    /// <summary>
    ///     Specifies the ORDER BY clause for the query using default ordering.
    /// </summary>
    /// <param name="columns">An array of column names to order by, emitted verbatim without a direction.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder OrderBy(params string[] columns);

    /// <summary>
    ///     Specifies the ORDER BY clause for a single column with a specified direction,
    ///     emitted as <c>{column} ASC</c> or <c>{column} DESC</c>.
    ///     The direction is not validated: an undefined value is emitted as its number.
    /// </summary>
    /// <param name="column">The column name to order by, emitted verbatim.</param>
    /// <param name="direction">The direction of ordering.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder OrderBy(string column, OrderDirection direction);

    /// <summary>
    ///     Limits the number of records returned by the query.
    /// </summary>
    /// <remarks>
    ///     The value is not validated. <see cref="GenerateSql" /> omits the LIMIT clause when it is 0, while
    ///     <see cref="GenerateFormattedSql" /> emits <c>LIMIT 0</c>. Calling it again replaces the value.
    /// </remarks>
    /// <param name="limit">The maximum number of records to return.</param>
    /// <returns>The current IQueryBuilder instance.</returns>
    IQueryBuilder SetLimit(int limit);

    /// <summary>
    ///     Generates the complete SQL query string based on the specified clauses.
    /// </summary>
    /// <returns>A SQL query string, on a single line.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <see cref="IQueryBuilderSetup.Setup" /> was not called, or when the table is undefined or not
    ///     present in the provided database information.
    /// </exception>
    string GenerateSql();

    /// <summary>
    ///     Generates a formatted SQL query with line breaks and indentation.
    /// </summary>
    /// <remarks>
    ///     Lines are separated by <see cref="Environment.NewLine" />, so the output differs between Windows and other
    ///     systems. Joins, and every filter after the first, are indented by two spaces.
    /// </remarks>
    /// <returns>A SQL query string.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <see cref="IQueryBuilderSetup.Setup" /> was not called, or when the table is undefined or not
    ///     present in the provided database information.
    /// </exception>
    string GenerateFormattedSql();
}
