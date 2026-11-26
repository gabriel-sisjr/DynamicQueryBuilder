namespace DynamicQueryBuilder.Models.Enums.SqlOperators;

/// <summary>
///     Represents SQL comparison operators.
/// </summary>
public enum ComparisonOperators
{
    /// <summary>Equal (=)</summary>
    EQUAL = 0,

    /// <summary>Not equal (&lt;&gt;)</summary>
    NOT_EQUAL = 1,

    /// <summary>Greater than (&gt;)</summary>
    GREATER_THAN = 2,

    /// <summary>Less than (&lt;)</summary>
    LESS_THAN = 3,

    /// <summary>Greater than or equal to (&gt;=)</summary>
    GREATER_OR_EQUAL = 4,

    /// <summary>Less than or equal to (&lt;=)</summary>
    LESS_OR_EQUAL = 5
}
