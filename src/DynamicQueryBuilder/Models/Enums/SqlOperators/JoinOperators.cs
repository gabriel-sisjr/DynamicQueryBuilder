namespace DynamicQueryBuilder.Models.Enums.SqlOperators;

/// <summary>
///     Represents SQL join operators.
/// </summary>
public enum JoinOperators
{
    /// <summary>INNER JOIN - Returns only records that have matching entries in both tables</summary>
    INNER_JOIN = 0,

    /// <summary>
    ///     LEFT JOIN (or LEFT OUTER JOIN) - Returns all records from the left table and the matching records from the
    ///     right table
    /// </summary>
    LEFT_JOIN = 1,

    /// <summary>
    ///     RIGHT JOIN (or RIGHT OUTER JOIN) - Returns all records from the right table and the matching records from the
    ///     left table
    /// </summary>
    RIGHT_JOIN = 2,

    /// <summary>FULL JOIN (or FULL OUTER JOIN) - Returns all records from both tables, with matching records when available</summary>
    FULL_JOIN = 3,

    /// <summary>CROSS JOIN - Produces a Cartesian product of the tables</summary>
    CROSS_JOIN = 4,

    /// <summary>SELF JOIN - Joins a table with itself</summary>
    SELF_JOIN = 5
}
