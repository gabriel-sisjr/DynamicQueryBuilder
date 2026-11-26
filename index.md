---
_layout: landing
---

# DynamicQueryBuilder

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/gabriel-sisjr/DynamicQueryBuilder/blob/develop/LICENSE)
[![License](https://img.shields.io/badge/License-AGPL-blue.svg)](https://github.com/gabriel-sisjr/DynamicQueryBuilder/blob/develop/LICENSE)

A .NET library for building SQL `SELECT` statements with a fluent API, plus a reader that lists the tables and columns of a database.

> [!WARNING]
> **Pre-alpha: don't use it in production.**
>
> - **Not on NuGet.** The library isn't published. The `DynamicQueryBuilder` package on nuget.org is an unrelated project.
> - **The API will change.** A rewrite is planned, and nothing here is stable.
> - **No protection against SQL injection.** Values, identifiers (table and column names), join conditions and operators are concatenated into the SQL text, with no parameters and no escaping. Never pass untrusted input. See the [security policy](https://github.com/gabriel-sisjr/DynamicQueryBuilder/blob/develop/SECURITY.md).
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

## 📚 Documentation

[Getting started](docs/getting-started.md) shows how to try the library from a clone of the repository, register it and build queries. The [API reference](xref:DynamicQueryBuilder.Builders.QueryBuilder) is generated from the source code.

## 💡 Examples

The repository's [examples](https://github.com/gabriel-sisjr/DynamicQueryBuilder/tree/develop/examples) folder has two projects. Both are part of `DynamicQueryBuilder.sln` and reference the library project directly. See [examples/README.md](https://github.com/gabriel-sisjr/DynamicQueryBuilder/blob/develop/examples/README.md) for how to run them.

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

- 📚 Check the [getting started guide](docs/getting-started.md)
- 🐛 Report issues on [GitHub Issues](https://github.com/gabriel-sisjr/DynamicQueryBuilder/issues)
- 💬 Join the discussion in [GitHub Discussions](https://github.com/gabriel-sisjr/DynamicQueryBuilder/discussions)

## 👤 Author

Created and maintained by [Gabriel Santana](https://github.com/gabriel-sisjr).

## 🙏 Acknowledgments

Special thanks to all contributors who have helped make this project better!
