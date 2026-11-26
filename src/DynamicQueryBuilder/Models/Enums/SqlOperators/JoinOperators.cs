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

    /// <summary>
    ///     FULL JOIN (or FULL OUTER JOIN) - Returns all records from both tables, with matching records when available.
    ///     MySQL has no FULL JOIN: it reads the emitted text as an alias (<c>FULL</c>) plus a plain join, or fails.
    /// </summary>
    FULL_JOIN = 3,

    /// <summary>
    ///     CROSS JOIN - Intended to produce a Cartesian product of the tables.
    ///     Known issue: the builder always emits it with an ON clause, which standard SQL does not allow for a
    ///     cross join, so the generated SQL is not a plain cross join.
    /// </summary>
    CROSS_JOIN = 4,

    /// <summary>
    ///     SELF JOIN - Intended to join a table with itself.
    ///     Known issue: the builder emits the text <c>SELF JOIN</c>, which is not an SQL join type. PostgreSQL and
    ///     MySQL read <c>SELF</c> as an alias for the FROM table, so the query fails when the condition names the FROM
    ///     table and otherwise runs as a plain inner join.
    /// </summary>
    SELF_JOIN = 5
}
