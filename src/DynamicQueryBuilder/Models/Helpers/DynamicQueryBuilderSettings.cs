using DynamicQueryBuilder.Models.Enums.Helpers.Databases;

namespace DynamicQueryBuilder.Models.Helpers;

/// <summary>
///     Configuration Object to initialize the library.
/// </summary>
/// <param name="ConnectionString">Database connection string.</param>
/// <param name="DatabaseDriver">Database Driver.</param>
public record DynamicQueryBuilderSettings(string ConnectionString, DatabaseDriver DatabaseDriver);
