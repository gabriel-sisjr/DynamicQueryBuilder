namespace DynamicQueryBuilder.Models.Enums.SqlOperators;

/// <summary>
///     Represents SQL null operators.
/// </summary>
public enum NullOperators
{
    /// <summary>IS NULL - Checks if a value is null</summary>
    IS_NULL = 0,

    /// <summary>IS NOT NULL - Checks if a value is not null</summary>
    IS_NOT_NULL = 1
}
