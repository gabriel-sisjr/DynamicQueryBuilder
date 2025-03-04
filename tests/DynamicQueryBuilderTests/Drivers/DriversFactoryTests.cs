using DynamicQueryBuilder.Drivers;
using DynamicQueryBuilder.Models.Enums.Helpers.Databases;
using FluentAssertions;
using MySql.Data.MySqlClient;
using Npgsql;
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
        var connection = DriversFactory.GetDatabaseConnection(DatabaseDriver.POSTGRESQL, PostgresConnectionString);

        // Assert
        connection.Should().BeOfType<NpgsqlConnection>();
        connection.ConnectionString.Should().Be(PostgresConnectionString);
    }

    [Fact]
    public void GetDatabaseConnection_WhenMySQL_ReturnsMySqlConnection()
    {
        // Act
        var connection = DriversFactory.GetDatabaseConnection(DatabaseDriver.MYSQL, MySqlConnectionString);

        // Assert
        connection.Should().BeOfType<MySqlConnection>();
        connection.ConnectionString.Should().Be(MySqlConnectionString);
    }

    [Fact]
    public void GetDatabaseConnection_WhenUnsupportedDriver_ThrowsNotSupportedException()
    {
        // Arrange
        var unsupportedDriver = (DatabaseDriver)999;

        // Act & Assert
        Action action = () => DriversFactory.GetDatabaseConnection(unsupportedDriver, PostgresConnectionString);
        action.Should().Throw<NotSupportedException>().WithMessage("UNSUPPORTED DATABASE DRIVER.");
    }

    [Fact]
    public void GetDatabaseConnection_WhenConnectionStringIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action action = () => DriversFactory.GetDatabaseConnection(DatabaseDriver.POSTGRESQL, null!);
        action.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'connectionString')");
    }

    [Fact]
    public void GetDatabaseConnection_WhenConnectionStringIsEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        Action action = () => DriversFactory.GetDatabaseConnection(DatabaseDriver.POSTGRESQL, string.Empty);
        action.Should().Throw<ArgumentException>().WithMessage("Connection string cannot be empty. (Parameter 'connectionString')");
    }

    [Fact]
    public void GetMetadataQuery_WhenPostgreSQL_ReturnsPostgreSQLQuery()
    {
        // Act
        var query = DriversFactory.GetMetadataQuery(DatabaseDriver.POSTGRESQL);

        // Assert
        query.Should().Contain("SELECT");
        query.Should().Contain("table_schema");
        query.Should().Contain("table_name");
        query.Should().Contain("column_name");
        query.Should().Contain("data_type");
        query.Should().Contain("information_schema.columns");
    }

    [Fact]
    public void GetMetadataQuery_WhenPostgreSQL_ContainsCorrectJoins()
    {
        // Act
        var query = DriversFactory.GetMetadataQuery(DatabaseDriver.POSTGRESQL);

        // Assert
        query.Should().Contain("LEFT JOIN");
        query.Should().Contain("FROM information_schema.table_constraints");
        query.Should().Contain("JOIN information_schema.key_column_usage");
        query.Should().Contain("JOIN information_schema.constraint_column_usage");
    }

    [Fact]
    public void GetMetadataQuery_WhenPostgreSQL_ExcludesSystemSchemas()
    {
        // Act
        var query = DriversFactory.GetMetadataQuery(DatabaseDriver.POSTGRESQL);

        // Assert
        query.Should().Contain("WHERE table_schema NOT IN ('pg_catalog', 'information_schema')");
    }

    [Fact]
    public void GetMetadataQuery_WhenMySQL_ReturnsMySQLQuery()
    {
        // Act
        var query = DriversFactory.GetMetadataQuery(DatabaseDriver.MYSQL);

        // Assert
        query.Should().Contain("SELECT");
        query.Should().Contain("TABLE_SCHEMA");
        query.Should().Contain("TABLE_NAME");
        query.Should().Contain("COLUMN_NAME");
        query.Should().Contain("COLUMN_TYPE");
        query.Should().Contain("INFORMATION_SCHEMA.COLUMNS");
    }

    [Fact]
    public void GetMetadataQuery_WhenMySQL_ContainsCorrectJoins()
    {
        // Act
        var query = DriversFactory.GetMetadataQuery(DatabaseDriver.MYSQL);

        // Assert
        query.Should().Contain("LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE");
    }

    [Fact]
    public void GetMetadataQuery_WhenMySQL_ExcludesSystemSchemas()
    {
        // Act
        var query = DriversFactory.GetMetadataQuery(DatabaseDriver.MYSQL);

        // Assert
        query.Should().Contain("WHERE TABLE_SCHEMA NOT IN ('mysql', 'information_schema', 'performance_schema')");
    }

    [Fact]
    public void GetMetadataQuery_WhenUnsupportedDriver_ThrowsNotSupportedException()
    {
        // Arrange
        var unsupportedDriver = (DatabaseDriver)999;

        // Act & Assert
        Action action = () => DriversFactory.GetMetadataQuery(unsupportedDriver);
        action.Should().Throw<NotSupportedException>().WithMessage("UNSUPPORTED DATABASE DRIVER.");
    }
}
