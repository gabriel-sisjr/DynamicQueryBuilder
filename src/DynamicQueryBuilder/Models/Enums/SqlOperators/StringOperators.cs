namespace DynamicQueryBuilder.Models.Enums.SqlOperators;

/// <summary>
///     Represents SQL string operators.
/// </summary>
public enum StringOperators
{
    /// <summary>LIKE - Searches for string patterns</summary>
    LIKE = 0,

    /// <summary>ILIKE - Searches for patterns without case sensitivity (PostgreSQL)</summary>
    ILIKE = 1,

    /// <summary>
    ///     Concatenation of strings (|| in PostgreSQL/Oracle, + in SQL Server).
    ///     Not supported yet: the FilterBy and OrFilterBy overloads throw <see cref="ArgumentOutOfRangeException" />
    ///     for this value, and no other method accepts it.
    /// </summary>
    CONCAT = 2
}
