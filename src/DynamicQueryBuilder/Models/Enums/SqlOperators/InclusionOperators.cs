namespace DynamicQueryBuilder.Models.Enums.SqlOperators;

/// <summary>
/// Represents SQL inclusion operators.
/// </summary>
public enum InclusionOperators
{
    /// <summary>IN - Checks if a value belongs to a set</summary>
    IN = 0,

    /// <summary>NOT IN - Checks if a value does not belong to a set</summary>
    NOT_IN = 1
}
