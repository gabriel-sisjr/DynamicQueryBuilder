using System.Text;
using DynamicQueryBuilder.Interfaces.Builders;
using DynamicQueryBuilder.Models.Enums.SqlOperators;

namespace DynamicQueryBuilder.Builders;

/// <summary>
///     Implements the IQueryBuilder interface to build SQL queries fluently.
/// </summary>
public class QueryBuilder : IQueryBuilder
{
    private readonly List<string> _columns = [];
    private readonly Dictionary<string, List<string>> _databaseInfo;
    private readonly List<FilterClause> _filters = [];
    private readonly List<string> _groups = [];
    private readonly List<string> _joins = [];
    private readonly List<string> _orders = [];
    private int? _limit;
    private string _table = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="QueryBuilder" /> class.
    /// </summary>
    /// <param name="databaseInfo">A dictionary containing table names and their column lists.</param>
    public QueryBuilder(Dictionary<string, List<string>> databaseInfo)
    {
        _databaseInfo = databaseInfo;
    }


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

    #region PRIVATE METHODS

    private QueryBuilder AddFilter(string condition, LogicalOperators joinOperator)
    {
        _filters.Add(new FilterClause(joinOperator, condition));
        return this;
    }

    #endregion

    #region FILTERS

    #region AND FILTERS

    /// <inheritdoc />
    public IQueryBuilder FilterBy(string column, string @operator, string value)
    {
        return AddFilter($"{column} {@operator} {value}", LogicalOperators.AND);
    }

    /// <inheritdoc />
    public IQueryBuilder FilterBy(string column, ComparisonOperators @operator, string value)
    {
        var opStr = @operator switch
        {
            ComparisonOperators.EQUAL => "=",
            ComparisonOperators.NOT_EQUAL => "<>",
            ComparisonOperators.GREATER_THAN => ">",
            ComparisonOperators.LESS_THAN => "<",
            ComparisonOperators.GREATER_OR_EQUAL => ">=",
            ComparisonOperators.LESS_OR_EQUAL => "<=",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), "Invalid comparison operator.")
        };

        return AddFilter($"{column} {opStr} {value}", LogicalOperators.AND);
    }

    /// <inheritdoc />
    public IQueryBuilder FilterBy(string column, InclusionOperators @operator, IEnumerable<string> values)
    {
        var opStr = @operator switch
        {
            InclusionOperators.IN => "IN",
            InclusionOperators.NOT_IN => "NOT IN",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), "Invalid inclusion operator.")
        };

        var valueList = "(" + string.Join(", ", values) + ")";
        return AddFilter($"{column} {opStr} {valueList}", LogicalOperators.AND);
    }

    /// <inheritdoc />
    public IQueryBuilder FilterBy(string column, NullOperators @operator)
    {
        var opStr = @operator switch
        {
            NullOperators.IS_NULL => "IS NULL",
            NullOperators.IS_NOT_NULL => "IS NOT NULL",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), "Invalid null operator.")
        };

        return AddFilter($"{column} {opStr}", LogicalOperators.AND);
    }

    /// <inheritdoc />
    public IQueryBuilder FilterBy(string column, RangeOperators @operator, string start, string end)
    {
        var opStr = @operator switch
        {
            RangeOperators.BETWEEN => "BETWEEN",
            RangeOperators.NOT_BETWEEN => "NOT BETWEEN",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), "Invalid range operator.")
        };

        return AddFilter($"{column} {opStr} {start} AND {end}", LogicalOperators.AND);
    }

    /// <inheritdoc />
    public IQueryBuilder FilterBy(string column, StringOperators @operator, string pattern)
    {
        var opStr = @operator switch
        {
            StringOperators.LIKE => "LIKE",
            StringOperators.ILIKE => "ILIKE",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), "Invalid string operator for filtering.")
        };

        return AddFilter($"{column} {opStr} {pattern}", LogicalOperators.AND);
    }

    #endregion

    #region OR FILTERS

    /// <inheritdoc />
    public IQueryBuilder OrFilterBy(string column, string @operator, string value)
    {
        return AddFilter($"{column} {@operator} {value}", LogicalOperators.OR);
    }

    /// <inheritdoc />
    public IQueryBuilder OrFilterBy(string column, ComparisonOperators @operator, string value)
    {
        var opStr = @operator switch
        {
            ComparisonOperators.EQUAL => "=",
            ComparisonOperators.NOT_EQUAL => "<>",
            ComparisonOperators.GREATER_THAN => ">",
            ComparisonOperators.LESS_THAN => "<",
            ComparisonOperators.GREATER_OR_EQUAL => ">=",
            ComparisonOperators.LESS_OR_EQUAL => "<=",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), "Invalid comparison operator.")
        };

        return AddFilter($"{column} {opStr} {value}", LogicalOperators.OR);
    }

    /// <inheritdoc />
    public IQueryBuilder OrFilterBy(string column, InclusionOperators @operator, IEnumerable<string> values)
    {
        var opStr = @operator switch
        {
            InclusionOperators.IN => "IN",
            InclusionOperators.NOT_IN => "NOT IN",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), "Invalid inclusion operator.")
        };

        var valueList = "(" + string.Join(", ", values) + ")";
        return AddFilter($"{column} {opStr} {valueList}", LogicalOperators.OR);
    }

    /// <inheritdoc />
    public IQueryBuilder OrFilterBy(string column, NullOperators @operator)
    {
        var opStr = @operator switch
        {
            NullOperators.IS_NULL => "IS NULL",
            NullOperators.IS_NOT_NULL => "IS NOT NULL",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), "Invalid null operator.")
        };

        return AddFilter($"{column} {opStr}", LogicalOperators.OR);
    }

    /// <inheritdoc />
    public IQueryBuilder OrFilterBy(string column, RangeOperators @operator, string start, string end)
    {
        var opStr = @operator switch
        {
            RangeOperators.BETWEEN => "BETWEEN",
            RangeOperators.NOT_BETWEEN => "NOT BETWEEN",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), "Invalid range operator.")
        };

        return AddFilter($"{column} {opStr} {start} AND {end}", LogicalOperators.OR);
    }

    /// <inheritdoc />
    public IQueryBuilder OrFilterBy(string column, StringOperators @operator, string pattern)
    {
        var opStr = @operator switch
        {
            StringOperators.LIKE => "LIKE",
            StringOperators.ILIKE => "ILIKE",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), "Invalid string operator for filtering.")
        };

        return AddFilter($"{column} {opStr} {pattern}", LogicalOperators.OR);
    }

    #endregion

    #endregion
}
