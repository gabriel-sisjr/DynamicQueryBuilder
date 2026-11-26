# Getting Started

> [!WARNING]
> **Pre-alpha: don't use it in production.**
>
> - **Not on NuGet.** The library isn't published. The `DynamicQueryBuilder` package on nuget.org is an unrelated project.
> - **The API will change.** A rewrite is planned, and nothing here is stable.
> - **No protection against SQL injection.** Values, identifiers (table and column names), join conditions and operators are concatenated into the SQL text, with no parameters and no escaping. Never pass untrusted input. See the [security policy](https://github.com/gabriel-sisjr/DynamicQueryBuilder/blob/develop/SECURITY.md).
> - **Almost no validation.** The only check on names is that the upper-cased `FROM` table is a key of the map passed to `Setup`. The column lists in that map are never read, and columns, joined tables, conditions and values aren't checked.

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

    /*
    * This method will return your schema tables metadata.
    */
    [HttpGet]
    public async Task<IActionResult> GetDatabaseMetadata()
    {
        var metadata = await _getDatabaseMetaData.GetDatabaseTablesMetaDataAsync();
        return Ok(metadata);
    }
}
```

The result describes your database schema, so don't expose it without authentication.

### Method Response

The method returns a dictionary of schema name → table name → a list of column entries, with names in the database's own casing. Each entry has:

- **Column name (`column`)**
- **Data type (`columnType`)**
- **`primaryKey`**, which despite its name doesn't tell you whether the column is part of the primary key. This is a known defect. On PostgreSQL it holds the column's default expression (`column_default`), or `null` when the column has no default. In the example below, `users.id` is a `serial` primary key and shows its `nextval(...)` default, `orders.id` is an identity primary key and shows `null`, and `orders.total_amount` isn't a key but shows its default, `0`. On MySQL the field is meant to hold `COLUMN_KEY` (`PRI`, `UNI`, `MUL` or empty), but the MySQL catalog query currently fails before returning anything, with `Column 'TABLE_SCHEMA' in field list is ambiguous`.
- **Foreign-key target (`relatedSchema`, `relatedTable`, `relatedColumn`)**, or `null` when the column isn't a foreign key or the connecting role doesn't own the tables. PostgreSQL shows foreign-key targets only to the tables' owner, even if another role has every table privilege.

Usually there's one entry per column. Columns in a composite foreign key, or in more than one foreign key, get one entry per target column, including wrong pairings: with a two-column key, each column is paired with both target columns.

The rows aren't sorted, so the column order isn't predictable: it can differ between two databases with the same tables.

#### Example of Returned Structure

The reader's result for a PostgreSQL database with these two tables, as JSON:

```sql
CREATE TABLE users (
    id    serial PRIMARY KEY,
    name  varchar(100) NOT NULL,
    email text UNIQUE
);
CREATE TABLE orders (
    id           bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id      integer NOT NULL REFERENCES users (id),
    total_amount numeric(10, 2) NOT NULL DEFAULT 0
);
```

```json
{
  "public": {
    "orders": [
      {
        "column": "id",
        "columnType": "bigint",
        "primaryKey": null,
        "relatedSchema": null,
        "relatedTable": null,
        "relatedColumn": null
      },
      {
        "column": "user_id",
        "columnType": "integer",
        "primaryKey": null,
        "relatedSchema": "public",
        "relatedTable": "users",
        "relatedColumn": "id"
      },
      {
        "column": "total_amount",
        "columnType": "numeric",
        "primaryKey": "0",
        "relatedSchema": null,
        "relatedTable": null,
        "relatedColumn": null
      }
    ],
    "users": [
      {
        "column": "id",
        "columnType": "integer",
        "primaryKey": "nextval('users_id_seq'::regclass)",
        "relatedSchema": null,
        "relatedTable": null,
        "relatedColumn": null
      },
      {
        "column": "email",
        "columnType": "text",
        "primaryKey": null,
        "relatedSchema": null,
        "relatedTable": null,
        "relatedColumn": null
      },
      {
        "column": "name",
        "columnType": "character varying",
        "primaryKey": null,
        "relatedSchema": null,
        "relatedTable": null,
        "relatedColumn": null
      }
    ]
  }
}
```

This isn't the shape `Setup` takes (upper-case table name → column names), so you build the `Setup` map yourself.

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
