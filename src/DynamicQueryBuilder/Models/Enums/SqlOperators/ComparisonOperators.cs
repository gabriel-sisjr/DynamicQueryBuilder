namespace DynamicQueryBuilder.Models.Enums.SqlOperators;

/// <summary>
/// Represents SQL comparison operators.
/// </summary>
public enum ComparisonOperators
{
    /// <summary>Equal (=)</summary>
    EQUAL = 0,

    /// <summary>Not Equal (<> or !=)</summary>
    NOT_EQUAL = 1,

    /// <summary>Greater than (>)</summary>
    GREATER_THAN = 2,

    /// <summary>Less than (<)</summary>
    LESS_THAN = 3,

    /// <summary>Greater than or equal to (>=)</summary>
    GREATER_OR_EQUAL = 4,

    /// <summary>Less than or equal to (<=)</summary>
    LESS_OR_EQUAL = 5
}
