namespace DynamicQueryBuilder.Models.Enums.SqlOperators;

/// <summary>
///     Represents a filter clause in SQL.
/// </summary>
/// <param name="JoinOperator">The logical operator to join the condition.</param>
/// <param name="Condition">The condition to filter the data.</param>
public record FilterClause(LogicalOperators JoinOperator, string Condition);
