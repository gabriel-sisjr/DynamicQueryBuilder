dotnet tool install -g docfx

# DynamicQueryBuilder

<!-- [![NuGet](https://img.shields.io/nuget/v/DynamicQueryBuilder.svg)](https://www.nuget.org/packages/DynamicQueryBuilder/) -->
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![License](https://img.shields.io/badge/License-AGPL-blue.svg)](LICENSE)

A powerful and flexible .NET library for building dynamic database queries with support for multiple database providers. DynamicQueryBuilder simplifies the process of creating dynamic queries by providing a type-safe, fluent interface while maintaining clean and maintainable code.

## 🚀 Features

### Core Features
- Fluent API for building SQL queries dynamically
- Type-safe query building
- Easy integration with dependency injection
- High performance and minimal overhead
- Extensive documentation and examples

### Database Support
- Multiple database providers:
  - PostgreSQL
  - MySQL
  - More coming soon!

### Query Capabilities
- Comprehensive SQL Operations:
  - `SELECT`, `JOIN`, `WHERE`, `GROUP BY`, `ORDER BY`, and `LIMIT` clauses
  - Various filtering options with AND/OR conditions
  - Generates both **formatted** and **unformatted** SQL queries

### Rich SQL Operators
- 🔄 Logical operators (AND, OR, NOT)
- 📝 String operators (LIKE, ILIKE, CONCAT)
- 🔗 Set operators (UNION, UNION ALL, INTERSECT, EXCEPT)
- 📊 Range operators (BETWEEN, NOT BETWEEN)
- 📋 Inclusion operators (IN, NOT IN)
- ❓ Null operators (IS NULL, IS NOT NULL)

### Additional Features
- 📊 Comprehensive database metadata retrieval
- 🔒 SQL injection protection
- 🎯 Built-in validation mechanisms

## 📦 Installation

Install the package via NuGet Package Manager:
```bash
PM> Install-Package DynamicQueryBuilder
```

Or using .NET CLI:
```bash
dotnet add package DynamicQueryBuilder
```

## 🛠 Usage

### Setting Up DynamicQueryBuilder

1. Add DynamicQueryBuilder to your services in `Program.cs`:
```csharp
builder.Services.AddDynamicQueryBuilder(builder.Configuration.GetConnectionString("YourConnectionString"));
```

2. Inject and use the service in your code:
```csharp
public class YourController : ControllerBase
{
    private readonly IGetDatabaseMetaData _getDatabaseMetaData;

    public YourController(IGetDatabaseMetaData getDatabaseMetaData)
    {
        _getDatabaseMetaData = getDatabaseMetaData;
    }

    public async Task<IActionResult> GetDatabaseMetadata()
    {
        var metadata = await _getDatabaseMetaData.GetDatabaseTablesMetaDataAsync(DatabaseDriver.POSTGRESQL);
        return Ok(metadata);
    }
}
```

### Building Queries

#### Basic SELECT Query
```csharp
var query = queryBuilder
    .FromTable("Users")
    .Columns("Id", "Name", "Email")
    .GenerateSql();
// Output: SELECT Id, Name, Email FROM USERS
```

#### Adding WHERE Filters
```csharp
var query = queryBuilder
    .FromTable("Users")
    .Columns("Id", "Name")
    .FilterBy("Age", ComparisonOperators.GREATER_THAN, "18")
    .GenerateSql();
// Output: SELECT Id, Name FROM USERS WHERE Age > 18
```

#### Using JOINs
```csharp
var query = queryBuilder
    .FromTable("Users")
    .Columns("Users.Id", "Users.Name", "Orders.TotalAmount")
    .Join(JoinOperators.INNER_JOIN, "Orders", "Users.Id = Orders.UserId")
    .GenerateSql();
// Output: SELECT Users.Id, Users.Name, Orders.TotalAmount FROM USERS INNER JOIN Orders ON Users.Id = Orders.UserId
```

## 📚 Documentation

For detailed documentation, examples, and API reference, visit our [documentation site](docs/).

## 💡 Examples

Check out the [examples](examples/) directory for complete working examples:
- `ClientWebAPI`: A sample Web API project demonstrating integration
- `ConsoleClient`: A console application showing basic usage

## ✅ Roadmap

- [ ] Add support for more database providers
- [ ] Enhance SQL injection protection mechanisms
- [ ] Add support for parameterized queries
- [ ] Extend documentation with more advanced examples
- [ ] Create comprehensive unit test suite
- [ ] Add validation for column names
- [ ] Implement caching mechanisms for better performance
- [ ] Add support for stored procedures

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 📝 License

This project is licensed under:
- **AGPL-3.0** license for open-source and personal projects
- **Commercial License** for commercial use (Contact gabriel.sistemasjr@gmail.com for details)

## 💬 Support

If you need help or have questions:
- 📚 Check the [documentation](docs/)
- 🐛 Report issues on [GitHub Issues](https://github.com/gabriel-sisjr/DynamicQueryBuilder/issues)
- 💬 Join the discussion in [GitHub Discussions](https://github.com/gabriel-sisjr/DynamicQueryBuilder/discussions)

## 👤 Author

Created and maintained by [Gabriel Santana](https://github.com/gabriel-sisjr).

## 🙏 Acknowledgments

Special thanks to all contributors who have helped make this project better!
