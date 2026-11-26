# DynamicQueryBuilder

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![License](https://img.shields.io/badge/License-AGPL-blue.svg)](LICENSE)

A .NET library for building SQL `SELECT` statements with a fluent API, plus a reader that lists the tables and columns of a database.

> [!WARNING]
> **Pre-alpha: don't use it in production.**
>
> - **Not on NuGet.** The library isn't published. The `DynamicQueryBuilder` package on nuget.org is an unrelated project.
> - **The API will change.** A rewrite is planned, and nothing here is stable.
> - **No protection against SQL injection.** Values, identifiers (table and column names), join conditions and operators are concatenated into the SQL text, with no parameters and no escaping. Never pass untrusted input. See [SECURITY.md](SECURITY.md).
> - **Almost no validation.** The only check on names is that the upper-cased `FROM` table is a key of the map passed to `Setup`. The column lists in that map are never read, and columns, joined tables, conditions and values aren't checked.

## 🚀 Features

### Core Features

- Fluent API that builds one SQL `SELECT` statement and returns it as a `string`
- Registration with dependency injection through `AddDynamicQueryBuilder`

### Database Support

- The metadata reader connects to PostgreSQL (Npgsql) or MySQL (MySql.Data). It works with PostgreSQL; with MySQL it currently fails (see Known limitations below).
- The query builder doesn't depend on the database: it emits the same SQL for both.

### Query Capabilities

- `SELECT`, `JOIN`, `WHERE`, `GROUP BY`, `ORDER BY` and `LIMIT` clauses
- Filters joined with `AND` (`FilterBy`) or `OR` (`OrFilterBy`)
- Generates both **formatted** and **unformatted** SQL queries

### SQL Operators

- Comparison operators (`=`, `<>`, `>`, `<`, `>=`, `<=`)
- Logical operators (`AND`, `OR`)
- String operators (`LIKE`, `ILIKE`)
- Range operators (`BETWEEN`, `NOT BETWEEN`)
- Inclusion operators (`IN`, `NOT IN`)
- Null operators (`IS NULL`, `IS NOT NULL`)
- Any other operator, passed as free text

### Additional Features

- Database metadata reader: the columns of every table the database user can access, grouped by schema and table, with each column's data type and foreign-key target (see Known limitations)

### Known limitations

- Filters are joined in call order, without parentheses: `a AND b OR c` is evaluated as `(a AND b) OR c`.
- The builder ignores the database driver, which only the metadata reader uses. It emits `ILIKE` and `FULL JOIN` for MySQL too, but MySQL has neither: `ILIKE` is a syntax error, and `FULL` is read as an alias for the `FROM` table, so the join either fails or runs as a plain join.
- `FromTable` upper-cases the table name using the current culture and emits it unquoted. PostgreSQL folds it back to lower case, so `FROM USERS` finds `users` but not a table created as `"Users"`. MySQL on Linux compares table names case-sensitively by default, so `FROM USERS` doesn't find `users`. There's no dedicated support for `schema.table` or aliases: free text such as `FromTable("public.users")` works only if the `Setup` map has the upper-cased string (`"PUBLIC.USERS"`) as a key.
- `JoinOperators.CROSS_JOIN` always adds an `ON` clause, which PostgreSQL rejects. `JoinOperators.SELF_JOIN` emits `SELF JOIN`, which isn't a join type: the database reads `SELF` as an alias for the `FROM` table, so the join fails if the condition names that table and otherwise runs as a plain join.
- `FilterBy` and `OrFilterBy` accept `StringOperators.CONCAT` but always throw `ArgumentOutOfRangeException` for it. `SetOperators`, `ArithmeticOperators` and `LogicalOperators.NOT` exist, but no builder method uses them.
- With MySQL the metadata reader fails: its catalog query throws `Column 'TABLE_SCHEMA' in field list is ambiguous`.
- Despite its name, the reader's `PrimaryKey` field doesn't tell you whether a column is part of the primary key. On PostgreSQL it holds the column's default expression.
- On PostgreSQL the foreign-key fields are `null` unless the connecting role owns the tables, even if it has every table privilege: `information_schema.constraint_column_usage` only shows tables owned by a currently enabled role.
- Columns in a composite foreign key, or in more than one foreign key, come back once per target column, including wrong pairings: with a two-column key, each column is paired with both target columns.
- The reader returns schema → table → columns, in the database's own casing. That isn't the shape `Setup` takes, so you build the `Setup` map yourself.

## 📦 Installation

DynamicQueryBuilder is not published on NuGet yet. The `DynamicQueryBuilder` package on nuget.org belongs to an unrelated project, so don't install it expecting this library.

To try it, clone the repository and add a project reference to `src/DynamicQueryBuilder/DynamicQueryBuilder.csproj`. The library targets `net8.0` and `net9.0`. From the folder that contains both the clone and your project:

```bash
git clone https://github.com/gabriel-sisjr/DynamicQueryBuilder.git
dotnet add YourApp/YourApp.csproj reference DynamicQueryBuilder/src/DynamicQueryBuilder/DynamicQueryBuilder.csproj
```

Or add the reference to your project file yourself:

```xml
<ItemGroup>
  <ProjectReference Include="../DynamicQueryBuilder/src/DynamicQueryBuilder/DynamicQueryBuilder.csproj" />
</ItemGroup>
```

## 🛠 Usage

### Setting Up DynamicQueryBuilder

1. Register the services in `Program.cs`. `AddDynamicQueryBuilder` takes a `DynamicQueryBuilderSettings` with the connection string and the database driver:

```csharp
using DynamicQueryBuilder;
using DynamicQueryBuilder.Models.Enums.Helpers.Databases;
using DynamicQueryBuilder.Models.Helpers;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DynamicQueryBuilder")
    ?? throw new InvalidOperationException("Connection string 'DynamicQueryBuilder' is not set.");

builder.Services.AddDynamicQueryBuilder(
    new DynamicQueryBuilderSettings(connectionString, DatabaseDriver.POSTGRESQL));
```

This registers two scoped services: the metadata reader (`IGetDatabaseMetaData`) and a query builder (`IQueryBuilderSetup`). `IQueryBuilder` itself isn't registered.

2. Inject the metadata reader where you need it. `GetDatabaseTablesMetaDataAsync` takes no arguments; the driver comes from the settings:

```csharp
using DynamicQueryBuilder.Interfaces.Helpers;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class YourController : ControllerBase
{
    private readonly IGetDatabaseMetaData _getDatabaseMetaData;

    public YourController(IGetDatabaseMetaData getDatabaseMetaData)
    {
        _getDatabaseMetaData = getDatabaseMetaData;
    }

    [HttpGet]
    public async Task<IActionResult> GetDatabaseMetadata()
    {
        var metadata = await _getDatabaseMetaData.GetDatabaseTablesMetaDataAsync();
        return Ok(metadata);
    }
}
```

The result describes your database schema, so don't expose it without authentication.

### Building Queries

You get a builder from `QueryBuilder.Create()`, from `new QueryBuilder()`, or as `IQueryBuilderSetup` from dependency injection. Either way, call `Setup` first: it takes a map of table names to column names. `GenerateSql` and `GenerateFormattedSql` throw `InvalidOperationException` if `Setup` wasn't called or if the `FROM` table isn't a key of the map. That's the only check: the column lists are never read.

`FromTable` upper-cases the table name before that lookup, so the keys must be upper-case. With `["Users"]` instead of `["USERS"]`, `GenerateSql` throws `Undefined or Invalid table.` A dictionary created with `StringComparer.OrdinalIgnoreCase` accepts either.

The samples below use `QueryBuilder.Create()` and this map:

```csharp
using DynamicQueryBuilder.Builders;
using DynamicQueryBuilder.Models.Enums.SqlOperators;

var tables = new Dictionary<string, List<string>>
{
    ["USERS"] = ["Id", "Name", "Email", "Age"],
    ["ORDERS"] = ["Id", "UserId", "TotalAmount"]
};
```

The builder is mutable. `Columns`, `Join`, the filters, `GroupBy` and `OrderBy` add to what's already there, while `FromTable`, `SetLimit` and `Setup` replace their value, so a reused builder carries parts of the previous query into the next one. The `IQueryBuilderSetup` from dependency injection is one instance per scope, so everything that resolves it while handling the same request shares it, filters included. When you need isolation, use `QueryBuilder.Create()` for each query.

#### Basic SELECT Query

```csharp
var sql = QueryBuilder.Create()
    .Setup(tables)
    .FromTable("Users")
    .Columns("Id", "Name", "Email")
    .GenerateSql();
// Output: SELECT Id, Name, Email FROM USERS
```

#### Adding WHERE Filters

```csharp
var sql = QueryBuilder.Create()
    .Setup(tables)
    .FromTable("Users")
    .Columns("Id", "Name")
    .FilterBy("Age", ComparisonOperators.GREATER_THAN, "18")
    .GenerateSql();
// Output: SELECT Id, Name FROM USERS WHERE Age > 18
```

The value is copied into the SQL as written. To compare with a string, pass the quotes yourself (`"'Alice'"`); nothing is escaped.

#### Using JOINs

```csharp
var sql = QueryBuilder.Create()
    .Setup(tables)
    .FromTable("Users")
    .Columns("Users.Id", "Users.Name", "Orders.TotalAmount")
    .Join(JoinOperators.INNER_JOIN, "Orders", "Users.Id = Orders.UserId")
    .GenerateSql();
// Output: SELECT Users.Id, Users.Name, Orders.TotalAmount FROM USERS INNER JOIN Orders ON Users.Id = Orders.UserId
```

Only `FROM` is checked against the map: the joined table, the column names and the condition are copied as written.

## 📚 Documentation

The [documentation site](https://gabriel-sisjr.github.io/DynamicQueryBuilder/) has a getting-started guide and the API reference.

## 💡 Examples

The [examples](examples/) folder has two projects. Both are part of `DynamicQueryBuilder.sln` and reference the library project directly. See [examples/README.md](examples/README.md) for how to run them.

- `ClientWebAPI`: an ASP.NET Core API that returns the metadata reader's output over HTTP. It has no authentication, so run it locally only. It needs a PostgreSQL connection string under `ConnectionStrings:DynamicQueryBuilder`: run `dotnet user-secrets set "ConnectionStrings:DynamicQueryBuilder" "<value>"` in `examples/ClientWebAPI/ClientWebAPI`, or set the `ConnectionStrings__DynamicQueryBuilder` environment variable. Without it, the app stops at startup.
- `ConsoleClient`: a placeholder that prints "Hello, World!". It doesn't use the library yet.

## ✅ Roadmap

A rewrite of the core is planned before the first release. It covers:

- [ ] A core that only generates SQL and its parameters: values sent as parameters, never as text
- [ ] Quoted identifiers, `schema.table` and aliases
- [ ] Grouped conditions: `AND`, `OR` and `NOT` with parentheses
- [ ] SQL generated for each database, starting with PostgreSQL and MySQL
- [ ] Every table and column checked against the schema read from the database
- [ ] Working metadata readers: real primary keys, composite foreign keys and a MySQL query that runs
- [ ] Tests that run the generated SQL against real databases
- [ ] More documentation and complete examples
- [ ] A first pre-release on NuGet, under a different package ID
- [ ] SQL Server support before 1.0
- [ ] Target frameworks `net10.0` and `net8.0`, plus `netstandard2.0` for the core

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 📝 License

This project is licensed under:
- **AGPL-3.0** license for open-source and personal projects
- **Commercial License** for commercial use (Contact gabriel.sistemasjr@gmail.com for details)

## 💬 Support

If you need help or have questions:
- 📚 Check the [documentation](https://gabriel-sisjr.github.io/DynamicQueryBuilder/)
- 🐛 Report issues on [GitHub Issues](https://github.com/gabriel-sisjr/DynamicQueryBuilder/issues)
- 💬 Join the discussion in [GitHub Discussions](https://github.com/gabriel-sisjr/DynamicQueryBuilder/discussions)

## 👤 Author

Created and maintained by [Gabriel Santana](https://github.com/gabriel-sisjr).

## 🙏 Acknowledgments

Special thanks to all contributors who have helped make this project better!
