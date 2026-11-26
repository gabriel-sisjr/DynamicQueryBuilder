# Introduction

DynamicQueryBuilder is a pre-alpha .NET library that builds SQL `SELECT` statements with a fluent API and reads table and column metadata from a database (PostgreSQL works; MySQL currently fails). It isn't published on NuGet, its API will change, and it concatenates every value and identifier into the SQL text, so never pass it untrusted input. To try it, see [Getting Started](getting-started.md).
