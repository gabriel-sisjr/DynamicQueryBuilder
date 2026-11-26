namespace DynamicQueryBuilder.Models.Helpers;

/// <summary>
///     Represents the metadata of one table column, as read from one row of the catalog query returned by
///     <see cref="DynamicQueryBuilder.Drivers.DriversFactory.GetMetadataQuery"/>: name, data type and foreign key
///     reference. The schema and table are the keys under which the column is returned.
/// </summary>
/// <remarks>
///     On PostgreSQL the foreign key values are null when the column isn't a foreign key or the connecting role
///     doesn't own the tables, because <c>information_schema.constraint_column_usage</c> only lists tables owned by a
///     currently enabled role. For a column in a multi-column foreign key, the PostgreSQL query returns one row per
///     referenced column, so <see cref="RelatedColumn" /> is not always the column this one references.
///     On MySQL the catalog query currently fails before returning any row (see <see cref="DynamicQueryBuilder.Models.Enums.Helpers.Databases.DatabaseDriver.MYSQL" />).
/// </remarks>
/// <param name="Column">The name of the column in the table.</param>
/// <param name="ColumnType">
///     The data type of the column: <c>data_type</c> on PostgreSQL. On MySQL it would hold <c>COLUMN_TYPE</c>, but the
///     MySQL query currently fails before returning rows.
/// </param>
/// <param name="PrimaryKey">
///     On PostgreSQL, despite its name, this is not a primary key indicator: it holds <c>column_default</c>, the
///     column's default expression, or null when the column has no default. On MySQL it would hold the
///     <c>COLUMN_KEY</c> value of <c>INFORMATION_SCHEMA.COLUMNS</c>, but the MySQL query currently fails before
///     returning rows.
/// </param>
/// <param name="RelatedSchema">
///     The schema of the related table if the catalog query reports a foreign key on this column; otherwise, null.
/// </param>
/// <param name="RelatedTable">
///     The name of the related table if the catalog query reports a foreign key on this column; otherwise, null.
/// </param>
/// <param name="RelatedColumn">
///     The name of the related column if the catalog query reports a foreign key on this column; otherwise, null.
/// </param>
public record DatabaseTablesMetaData(
    string Column,
    string ColumnType,
    string? PrimaryKey,
    string? RelatedSchema,
    string? RelatedTable,
    string? RelatedColumn
);
