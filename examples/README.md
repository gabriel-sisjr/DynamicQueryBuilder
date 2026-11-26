# Examples

Both example projects are part of `DynamicQueryBuilder.sln` and reference the library in
`src/DynamicQueryBuilder` with a `ProjectReference`, so they always build against the code in this
repository, not against a NuGet package.

> **Local use only.** `ClientWebAPI` has no authentication and returns the structure of the database to
> anyone who can reach it. Do not deploy it or expose it to a network.

## ClientWebAPI

An ASP.NET Core Web API targeting `net9.0`. At startup, `Program.cs` reads the connection string
`ConnectionStrings:DynamicQueryBuilder` and registers the library with `AddDynamicQueryBuilder`. The driver is
fixed to PostgreSQL in code, so the value must be a PostgreSQL connection string in
[Npgsql format](https://www.npgsql.org/doc/connection-string-parameters.html).

It has a single API endpoint, `/api/DynamicQueryBuilder`, in `Controllers/DynamicQueryBuilderController.cs`. It
calls `IGetDatabaseMetaData.GetDatabaseTablesMetaDataAsync()` and returns the result as JSON: schema name, then table
name, then one object per row of the library's catalog query, with the fields `column`, `columnType`, `primaryKey`,
`relatedSchema`, `relatedTable` and `relatedColumn`. That is usually one object per column, but a column that is
part of a multi-column foreign key, or of more than one foreign key, appears more than once. Every schema except
`pg_catalog` and `information_schema` is included, limited to the tables the connecting user can see. The query
builder itself is not used by this example.

What to expect today:

- There is no authentication or authorization; every request is answered.
- The action has no HTTP method attribute, so `GET`, `POST`, `PUT`, `DELETE` and `PATCH` all return the same
  response.
- On PostgreSQL, `primaryKey` holds the column's default expression (for example
  `nextval('employees_id_seq'::regclass)`), not whether the column is a primary key. This is a known library defect.
- `relatedSchema`, `relatedTable` and `relatedColumn` are null when the column isn't a foreign key or the
  connecting role doesn't own the tables, even if it has every other privilege. For a multi-column foreign key, each column is listed once per
  referenced column, so some of those rows name the wrong `relatedColumn`.
- In the `Development` environment the app maps `/openapi/v1.json`, but that request currently fails with
  HTTP 500 (`Unsupported HTTP method`) because the action has no HTTP method attribute.
- `WeatherForecast.cs` and `ClientWebAPI.http` are leftovers from the project template. The request in
  `ClientWebAPI.http` targets `/weatherforecast/`, which does not exist and returns 404.

## ConsoleClient

A placeholder console app targeting `net9.0` that prints `Hello, World!`. It references the library but does not
use it yet.

## Prerequisites

- .NET SDK 9.0 or later. The examples target `net9.0`. Building the whole solution also builds the library and
  the tests for `net8.0` and `net9.0`, and running the `net8.0` tests also needs the .NET 8 runtime.
- For `ClientWebAPI`, a PostgreSQL server you can connect to.

## Configure the connection string

`appsettings.json` keeps the key `ConnectionStrings:DynamicQueryBuilder` with an empty value on purpose. Do not
put a real connection string in it. Use one of the options below, where `<value>` is your connection string.

Typing the connection string as a command argument puts its password in your shell history. The bash/zsh forms
below read it with `read -rs` instead, so it is neither echoed nor recorded.

**User secrets**, stored unencrypted, as plain JSON, in your user profile, outside the repository:

```bash
cd examples/ClientWebAPI/ClientWebAPI
dotnet user-secrets set "ConnectionStrings:DynamicQueryBuilder" "<value>"

# Same, without the value in shell history (bash/zsh):
read -rs DQB_CS && dotnet user-secrets set "ConnectionStrings:DynamicQueryBuilder" "$DQB_CS"; unset DQB_CS
```

User secrets are loaded only when the environment is `Development`. Both launch profiles in
`Properties/launchSettings.json` set `ASPNETCORE_ENVIRONMENT=Development`, so `dotnet run` picks them up.

**Environment variable**, which works in any environment:

```bash
read -rs DQB_CS && export ConnectionStrings__DynamicQueryBuilder="$DQB_CS"; unset DQB_CS
```

```powershell
# PowerShell also records typed commands in its history.
$env:ConnectionStrings__DynamicQueryBuilder = "<value>"
```

If the value is missing, empty or whitespace, the app stops at startup with an `InvalidOperationException` that
names both options.

## Build and run

From the repository root:

```bash
dotnet build DynamicQueryBuilder.sln

dotnet run --project examples/ClientWebAPI/ClientWebAPI
# in another terminal:
curl http://localhost:5147/api/DynamicQueryBuilder

dotnet run --project examples/ConsoleClient
```

`dotnet run` uses the first launch profile, `http`, which listens on `http://localhost:5147`.
