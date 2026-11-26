# Security Policy

## Supported versions

DynamicQueryBuilder is pre-alpha. Nothing has been released: there are no tags, no GitHub releases and no NuGet package published from this repository.

| Version                                  | Receives security fixes |
| ---------------------------------------- | ----------------------- |
| Default branch (currently `develop`)     | Yes, best effort        |
| Any other branch, fork or older commit   | No                      |

The `DynamicQueryBuilder` package on nuget.org belongs to an unrelated project. Report problems in that package to its own maintainers, not here.

## Reporting a vulnerability

Please do not report security problems through public issues, discussions or pull requests.

Report them privately through GitHub private vulnerability reporting:

<https://github.com/gabriel-sisjr/DynamicQueryBuilder/security/advisories/new>

A useful report includes:

- the commit or branch you tested;
- the affected API or file;
- steps to reproduce, or a minimal proof of concept;
- the impact you expect (for example, data read or modified);
- a suggested fix, if you have one.

## What to expect

This project is maintained on a best-effort basis, with no guaranteed response times. As a goal:

- an acknowledgement within 7 days;
- a first assessment within 30 days;
- fixes land on the default branch; their timing depends on severity and on the planned rewrite described below.

We coordinate disclosure with you through the private advisory and credit you in it, unless you prefer not to be named.

## Known limitations

These are known, documented problems of the current pre-alpha. They do not need a private report.

- **SQL injection.** The current code builds SQL by concatenating values, identifiers (table and column names), join conditions and operators into the SQL text. It is open to SQL injection until the planned rewrite. Parameters will fix values only; the rewrite must also quote and validate identifiers, operators and join conditions.
- **Do not use it with untrusted input.** Nothing that comes from users, HTTP requests, files or any other external source should reach the query builder.

The following defects affect you even when every input is trusted:

- **`AND`/`OR` filters are not parenthesized.** Filters are joined as written, so `TenantId = 42 AND Name = 'bob' OR Name = 'alice'` returns the `alice` rows of every tenant, not only tenant 42.
- **`DynamicQueryBuilderSettings.ToString()` prints the full connection string, password included.** It is a positional record, so logging or formatting an instance leaks the credentials. Do not log it.
- **The builder is scoped and stateful.** The DI container registers it as scoped, and it accumulates clauses. Every consumer resolved in the same scope (for example, the same HTTP request) shares one instance, so filters and other clauses leak from one query into the next.
