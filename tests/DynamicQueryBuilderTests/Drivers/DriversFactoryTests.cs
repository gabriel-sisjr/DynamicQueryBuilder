using System.Data;
using DynamicQueryBuilder.Drivers;
using DynamicQueryBuilder.Models.Enums.Helpers.Databases;
using MySql.Data.MySqlClient;
using Npgsql;
using Shouldly;
using Xunit;

namespace DynamicQueryBuilderTests.Drivers;

public class DriversFactoryTests
{
    private const string PostgresConnectionString = "Host=localhost;Database=testdb;Username=test;Password=test";
    private const string MySqlConnectionString = "server=localhost;database=testdb;user id=test;password=test";

    [Fact]
    public void GetDatabaseConnection_WhenPostgreSQL_ReturnsNpgsqlConnection()
    {
        // Act
        IDbConnection? connection =
            DriversFactory.GetDatabaseConnection(DatabaseDriver.POSTGRESQL, PostgresConnectionString);

        // Assert
        connection.ShouldBeOfType<NpgsqlConnection>();
        connection.ConnectionString.ShouldBe(PostgresConnectionString);
    }

    [Fact]
    public void GetDatabaseConnection_WhenMySQL_ReturnsMySqlConnection()
    {
        // Act
        IDbConnection? connection = DriversFactory.GetDatabaseConnection(DatabaseDriver.MYSQL, MySqlConnectionString);

        // Assert
        connection.ShouldBeOfType<MySqlConnection>();
        connection.ConnectionString.ShouldBe(MySqlConnectionString);
    }

    [Fact]
    public void GetDatabaseConnection_WhenUnsupportedDriver_ThrowsNotSupportedException()
    {
        // Arrange
        var unsupportedDriver = (DatabaseDriver)999;

        // Act & Assert
        Action action = () => DriversFactory.GetDatabaseConnection(unsupportedDriver, PostgresConnectionString);
        action.ShouldThrow<NotSupportedException>("UNSUPPORTED DATABASE DRIVER.");
    }

    [Fact]
    public void GetDatabaseConnection_WhenConnectionStringIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action action = () => DriversFactory.GetDatabaseConnection(DatabaseDriver.POSTGRESQL, null!);
        action.ShouldThrow<ArgumentNullException>("Value cannot be null. (Parameter 'connectionString')");
    }

    [Fact]
    public void GetDatabaseConnection_WhenConnectionStringIsEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        Action action = () => DriversFactory.GetDatabaseConnection(DatabaseDriver.POSTGRESQL, string.Empty);
        action.ShouldThrow<ArgumentException>("Connection string cannot be empty. (Parameter 'connectionString')");
    }

    [Fact]
    public void GetMetadataQuery_WhenPostgreSQL_ReturnsPostgreSQLQuery()
    {
        // Act
        var query = DriversFactory.GetMetadataQuery(DatabaseDriver.POSTGRESQL);

        // Assert
        query.ShouldContain("SELECT");
        query.ShouldContain("table_schema");
        query.ShouldContain("table_name");
        query.ShouldContain("column_name");
        query.ShouldContain("data_type");
        query.ShouldContain("information_schema.columns");
    }

    [Fact]
    public void GetMetadataQuery_WhenPostgreSQL_ContainsCorrectJoins()
    {
        // Act
        var query = DriversFactory.GetMetadataQuery(DatabaseDriver.POSTGRESQL);

        // Assert
        query.ShouldContain("LEFT JOIN");
        query.ShouldContain("FROM information_schema.table_constraints");
        query.ShouldContain("JOIN information_schema.key_column_usage");
        query.ShouldContain("JOIN information_schema.constraint_column_usage");
    }

    [Fact]
    public void GetMetadataQuery_WhenPostgreSQL_ExcludesSystemSchemas()
    {
        // Act
        var query = DriversFactory.GetMetadataQuery(DatabaseDriver.POSTGRESQL);

        // Assert
        query.ShouldContain("WHERE table_schema NOT IN ('pg_catalog', 'information_schema')");
    }

    [Fact]
    public void GetMetadataQuery_WhenMySQL_ReturnsMySQLQuery()
    {
        // Act
        var query = DriversFactory.GetMetadataQuery(DatabaseDriver.MYSQL);

        // Assert
        query.ShouldContain("SELECT");
        query.ShouldContain("TABLE_SCHEMA");
        query.ShouldContain("TABLE_NAME");
        query.ShouldContain("COLUMN_NAME");
        query.ShouldContain("COLUMN_TYPE");
        query.ShouldContain("INFORMATION_SCHEMA.COLUMNS");
    }

    [Fact]
    public void GetMetadataQuery_WhenMySQL_ContainsCorrectJoins()
    {
        // Act
        var query = DriversFactory.GetMetadataQuery(DatabaseDriver.MYSQL);

        // Assert
        query.ShouldContain("LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE");
    }

    [Fact]
    public void GetMetadataQuery_WhenMySQL_ExcludesSystemSchemas()
    {
        // Act
        var query = DriversFactory.GetMetadataQuery(DatabaseDriver.MYSQL);

        // Assert
        query.ShouldContain("WHERE TABLE_SCHEMA NOT IN ('mysql', 'information_schema', 'performance_schema')");
    }

    [Fact]
    public void GetMetadataQuery_WhenUnsupportedDriver_ThrowsNotSupportedException()
    {
        // Arrange
        var unsupportedDriver = (DatabaseDriver)999;

        // Act & Assert
        Action action = () => DriversFactory.GetMetadataQuery(unsupportedDriver);
        action.ShouldThrow<NotSupportedException>("UNSUPPORTED DATABASE DRIVER.");
    }
}
