namespace DynamicQueryBuilder.Models.Helpers;

/// <summary>
///     Represents the metadata of a database table, including schema, columns, primary key, and foreign key relationships.
/// </summary>
/// <param name="Column">The name of the column in the table.</param>
/// <param name="ColumnType">The data type of the column.</param>
/// <param name="PrimaryKey">The name of the primary key column if applicable; otherwise, null.</param>
/// <param name="RelatedSchema">The schema of the related table if there is a foreign key relationship; otherwise, null.</param>
/// <param name="RelatedTable">The name of the related table if there is a foreign key relationship; otherwise, null.</param>
/// <param name="RelatedColumn">The name of the related column if there is a foreign key relationship; otherwise, null.</param>
public record DatabaseTablesMetaData(
    string Column,
    string ColumnType,
    string? PrimaryKey,
    string? RelatedSchema,
    string? RelatedTable,
    string? RelatedColumn
);
