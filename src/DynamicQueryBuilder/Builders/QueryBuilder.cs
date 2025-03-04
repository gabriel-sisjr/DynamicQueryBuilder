using System.Text;
using DynamicQueryBuilder.Interfaces.Builders;
using DynamicQueryBuilder.Models.Enums.SqlOperators;

namespace DynamicQueryBuilder.Builders;

/// <summary>
///     Implements the IQueryBuilder interface to build SQL queries fluently.
/// </summary>
public partial class QueryBuilder : IQueryBuilderSetup, IQueryBuilder
{
    private readonly List<string> _columns = [];
    private readonly List<FilterClause> _filters = [];
    private readonly List<string> _groups = [];
    private readonly List<string> _joins = [];
    private readonly List<string> _orders = [];
    private Dictionary<string, List<string>>? _databaseInfo;
    private int? _limit;
    private string _table = string.Empty;

    /// <inheritdoc />
    public IQueryBuilder FromTable(string table)
    {
        _table = table.ToUpper();
        return this;
    }

    /// <inheritdoc />
    public IQueryBuilder Columns(params string[] columns)
    {
        _columns.AddRange(columns);
        return this;
    }

    /// <inheritdoc />
    public IQueryBuilder Join(JoinOperators type, string table, string condition)
    {
        var joinClause = type switch
        {
            JoinOperators.INNER_JOIN => "INNER JOIN",
            JoinOperators.LEFT_JOIN => "LEFT JOIN",
            JoinOperators.RIGHT_JOIN => "RIGHT JOIN",
            JoinOperators.FULL_JOIN => "FULL JOIN",
            JoinOperators.CROSS_JOIN => "CROSS JOIN",
            JoinOperators.SELF_JOIN => "SELF JOIN",
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Invalid join operator.")
        };

        _joins.Add($"{joinClause} {table} ON {condition}");
        return this;
    }

    /// <inheritdoc />
    public IQueryBuilder GroupBy(params string[] columns)
    {
        _groups.AddRange(columns);
        return this;
    }

    /// <inheritdoc />
    public IQueryBuilder OrderBy(params string[] columns)
    {
        _orders.AddRange(columns);
        return this;
    }

    /// <inheritdoc />
    public IQueryBuilder OrderBy(string column, OrderDirection direction)
    {
        _orders.Add($"{column} {direction}");
        return this;
    }

    /// <inheritdoc />
    public IQueryBuilder SetLimit(int limit)
    {
        _limit = limit;
        return this;
    }

    /// <inheritdoc />
    public string GenerateSql()
    {
        if (_databaseInfo == null)
            throw new InvalidOperationException("Setup must be called before building the query.");

        if (string.IsNullOrEmpty(_table) || !_databaseInfo.ContainsKey(_table))
            throw new InvalidOperationException("Undefined or Invalid table.");

        var sql = new StringBuilder();
        sql.Append("SELECT ");
        sql.Append(_columns.Count != 0 ? string.Join(", ", _columns) : "*");
        sql.Append($" FROM {_table}");

        if (_joins.Count != 0)
            sql.Append(" " + string.Join(" ", _joins));

        if (_filters.Count != 0)
        {
            sql.Append(" WHERE ");
            // Print the first filter without a preceding operator.
            sql.Append(_filters[0].Condition);
            // Append each subsequent filter with its join operator.
            foreach (FilterClause filter in _filters.Skip(1))
                sql.Append($" {filter.JoinOperator} {filter.Condition}");
        }

        if (_groups.Count != 0)
            sql.Append(" GROUP BY " + string.Join(", ", _groups));

        if (_orders.Count != 0)
            sql.Append(" ORDER BY " + string.Join(", ", _orders));

        if (_limit is not (null or 0))
            sql.Append($" LIMIT {_limit}");

        return sql.ToString();
    }

    /// <inheritdoc />
    public string GenerateFormattedSql()
    {
        if (_databaseInfo == null)
            throw new InvalidOperationException("Setup must be called before building the query.");

        if (string.IsNullOrEmpty(_table) || !_databaseInfo.ContainsKey(_table))
            throw new InvalidOperationException("Undefined or Invalid table.");

        var sql = new StringBuilder();
        sql.Append("SELECT ");
        sql.AppendLine(_columns.Count != 0 ? string.Join(", ", _columns) : "*");
        sql.Append("FROM " + _table);

        if (_joins.Count != 0)
        {
            sql.AppendLine();
            sql.Append("  " + string.Join(Environment.NewLine + "  ", _joins));
        }

        if (_filters.Count != 0)
        {
            sql.AppendLine();
            sql.Append("WHERE ");
            sql.Append(_filters[0].Condition);
            foreach (FilterClause filter in _filters.Skip(1))
            {
                sql.AppendLine();
                sql.Append($"  {filter.JoinOperator} {filter.Condition}");
            }
        }

        if (_groups.Count != 0)
        {
            sql.AppendLine();
            sql.Append("GROUP BY " + string.Join(", ", _groups));
        }

        if (_orders.Count != 0)
        {
            sql.AppendLine();
            sql.Append("ORDER BY " + string.Join(", ", _orders));
        }

        if (_limit.HasValue)
        {
            sql.AppendLine();
            sql.Append("LIMIT " + _limit);
        }

        return sql.ToString();
    }

    /// <inheritdoc />
    public IQueryBuilder Setup(Dictionary<string, List<string>> databaseInfo)
    {
        _databaseInfo = databaseInfo;
        return this;
    }

    /// <summary>
    ///     Factory method to start the builder.
    /// </summary>
    /// <returns>A <see cref="QueryBuilder" /> stance.</returns>
    public static IQueryBuilderSetup Create()
    {
        return new QueryBuilder();
    }
}
