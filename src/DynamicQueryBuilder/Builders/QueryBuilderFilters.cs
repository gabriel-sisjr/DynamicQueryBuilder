using DynamicQueryBuilder.Interfaces.Builders;
using DynamicQueryBuilder.Models.Enums.SqlOperators;

namespace DynamicQueryBuilder.Builders;

/// <summary>
///     Implements the IQueryBuilder interface to build SQL queries fluently.
/// </summary>
public partial class QueryBuilder
{
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

    #region PRIVATE METHODS

    private QueryBuilder AddFilter(string condition, LogicalOperators joinOperator)
    {
        _filters.Add(new FilterClause(joinOperator, condition));
        return this;
    }

    #endregion
}
