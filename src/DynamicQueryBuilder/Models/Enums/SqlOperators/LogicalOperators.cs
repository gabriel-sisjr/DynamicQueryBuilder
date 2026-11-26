namespace DynamicQueryBuilder.Models.Enums.SqlOperators;

/// <summary>
///     Represents SQL logical operators.
/// </summary>
/// <remarks>
///     The query builder uses <see cref="AND" /> and <see cref="OR" /> to join filters and emits the member name.
/// </remarks>
public enum LogicalOperators
{
    /// <summary>Logical operator AND</summary>
    AND = 0,

    /// <summary>Logical operator OR</summary>
    OR = 1,

    /// <summary>Logical operator NOT. Not used by the query builder yet.</summary>
    NOT = 2
}
