namespace DynamicQueryBuilder.Models.Enums.SqlOperators;

/// <summary>
///     Represents SQL range operators.
/// </summary>
public enum RangeOperators
{
    /// <summary>BETWEEN - Checks if the value is within a range</summary>
    BETWEEN = 0,

    /// <summary>NOT BETWEEN - Checks if the value is outside a range</summary>
    NOT_BETWEEN = 1
}
