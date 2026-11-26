namespace DynamicQueryBuilder.Models.Enums.SqlOperators;

/// <summary>
///     Represents a filter clause in SQL.
/// </summary>
/// <remarks>
///     Used internally by the query builder to store its filters; no member of
///     <see cref="DynamicQueryBuilder.Builders.QueryBuilder" /> or
///     <see cref="DynamicQueryBuilder.Interfaces.Builders.IQueryBuilder" /> accepts or returns it.
/// </remarks>
/// <param name="JoinOperator">
///     The logical operator to join the condition. It is not emitted for the first filter.
/// </param>
/// <param name="Condition">The condition to filter the data, as the SQL text emitted.</param>
public record FilterClause(LogicalOperators JoinOperator, string Condition);
