namespace DynamicQueryBuilder.Models.Enums.SqlOperators;

/// <summary>
///     Represents SQL set operators.
/// </summary>
/// <remarks>Not used by the query builder yet: no method accepts these values.</remarks>
public enum SetOperators
{
    /// <summary>UNION - Combines and returns distinct results from two queries</summary>
    UNION = 0,

    /// <summary>UNION ALL - Combines results, including duplicates</summary>
    UNION_ALL = 1,

    /// <summary>INTERSECT - Returns records common to both queries</summary>
    INTERSECT = 2,

    /// <summary>EXCEPT - Returns records that are in the first query but not in the second</summary>
    EXCEPT = 3
}
