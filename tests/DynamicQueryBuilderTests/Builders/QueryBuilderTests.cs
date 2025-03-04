using DynamicQueryBuilder.Builders;
using DynamicQueryBuilder.Interfaces.Builders;
using DynamicQueryBuilder.Models.Enums.SqlOperators;
using Xunit;

namespace DynamicQueryBuilderTests.Builders;

public class QueryBuilderTests
{
    private readonly Dictionary<string, List<string>> _dbInfo = new()
    {
        { "EMPLOYEES", ["ID", "NAME", "AGE", "CITY", "DEPARTMENT_ID"] },
        { "DEPARTMENTS", ["ID", "DEPT_NAME"] }
    };

    [Fact]
    public void TestSimpleSelect()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo).FromTable("employees");
        var sql = qb.GenerateSql();

        // Assert
        Assert.Equal("SELECT * FROM EMPLOYEES", sql);
    }

    [Fact]
    public void TestSelectColumns()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .Columns("ID", "NAME");
        var sql = qb.GenerateSql();

        // Assert
        Assert.Equal("SELECT ID, NAME FROM EMPLOYEES", sql);
    }

    [Fact]
    public void TestJoinClause()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .Columns("EMPLOYEES.ID", "EMPLOYEES.NAME", "DEPARTMENTS.DEPT_NAME")
            .Join(JoinOperators.INNER_JOIN, "DEPARTMENTS", "EMPLOYEES.DEPARTMENT_ID = DEPARTMENTS.ID");
        var sql = qb.GenerateSql();

        // Assert
        const string expectedSql =
            "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, DEPARTMENTS.DEPT_NAME FROM EMPLOYEES INNER JOIN DEPARTMENTS ON EMPLOYEES.DEPARTMENT_ID = DEPARTMENTS.ID";
        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void TestFilterByComparison()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .FilterBy("AGE", ComparisonOperators.GREATER_THAN, "30");
        var sql = qb.GenerateSql();

        // Assert
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE > 30", sql);
    }

    [Fact]
    public void TestAndMultipleFilters()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .FilterBy("CITY", StringOperators.LIKE, "'New%'")
            .FilterBy("AGE", ComparisonOperators.GREATER_OR_EQUAL, "18");
        var sql = qb.GenerateSql();

        // Assert
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE CITY LIKE 'New%' AND AGE >= 18", sql);
    }

    [Fact]
    public void TestOrMultipleFilters()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .FilterBy("AGE", ComparisonOperators.GREATER_THAN, "25")
            .OrFilterBy("DEPARTMENT", ComparisonOperators.EQUAL, "'Sales'");
        var sql = qb.GenerateSql();

        // Assert
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE > 25 OR DEPARTMENT = 'Sales'", sql);
    }

    [Fact]
    public void TestGroupOrderLimit()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .Columns("DEPARTMENT", "COUNT(ID) AS EmployeeCount")
            .GroupBy("DEPARTMENT")
            .OrderBy("EmployeeCount", OrderDirection.DESC)
            .SetLimit(10);
        var sql = qb.GenerateSql();

        // Assert
        const string expectedSql =
            "SELECT DEPARTMENT, COUNT(ID) AS EmployeeCount FROM EMPLOYEES GROUP BY DEPARTMENT ORDER BY EmployeeCount DESC LIMIT 10";
        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void TestFormattedSql()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .Columns("EMPLOYEES.ID", "EMPLOYEES.NAME", "DEPARTMENTS.DEPT_NAME")
            .Join(JoinOperators.INNER_JOIN, "DEPARTMENTS", "EMPLOYEES.DEPARTMENT_ID = DEPARTMENTS.ID")
            .FilterBy("AGE", ComparisonOperators.GREATER_THAN, "30")
            .GroupBy("DEPARTMENTS.DEPT_NAME")
            .OrderBy("EMPLOYEES.NAME", OrderDirection.ASC)
            .SetLimit(5);
        var formattedSql = qb.GenerateFormattedSql();

        // Assert (check if the formatted string contains the expected parts)
        Assert.Contains("SELECT", formattedSql);
        Assert.Contains("FROM EMPLOYEES", formattedSql);
        Assert.Contains("INNER JOIN DEPARTMENTS ON EMPLOYEES.DEPARTMENT_ID = DEPARTMENTS.ID", formattedSql);
        Assert.Contains("WHERE AGE > 30", formattedSql);
        Assert.Contains("GROUP BY DEPARTMENTS.DEPT_NAME", formattedSql);
        Assert.Contains("ORDER BY EMPLOYEES.NAME ASC", formattedSql);
        Assert.Contains("LIMIT 5", formattedSql);
    }

    [Fact]
    public void TestInclusionFilter()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .FilterBy("DEPARTMENT", InclusionOperators.IN, new List<string> { "'Sales'", "'Marketing'" });
        var sql = qb.GenerateSql();

        // Assert
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE DEPARTMENT IN ('Sales', 'Marketing')", sql);
    }

    [Fact]
    public void TestNullFilter()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .FilterBy("AGE", NullOperators.IS_NULL);
        var sql = qb.GenerateSql();

        // Assert
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE IS NULL", sql);
    }

    [Fact]
    public void TestRangeFilter()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .FilterBy("AGE", RangeOperators.BETWEEN, "25", "35");
        var sql = qb.GenerateSql();

        // Assert
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE BETWEEN 25 AND 35", sql);
    }

    [Fact]
    public void TestStringFilter()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .FilterBy("NAME", StringOperators.LIKE, "'A%'");
        var sql = qb.GenerateSql();

        // Assert
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE NAME LIKE 'A%'", sql);
    }

    [Fact]
    public void TestUndefinedTableThrowsException()
    {
        // Act
        var qb = new QueryBuilder(_dbInfo);

        // Assert
        Assert.Throws<InvalidOperationException>(() => qb.GenerateSql());
    }

    [Fact]
    public void TestInvalidTableThrowsException()
    {
        // Act
        IQueryBuilder qb = new QueryBuilder(_dbInfo).FromTable("Other table name");

        // Assert
        Assert.Throws<InvalidOperationException>(() => qb.GenerateSql());
    }

    [Fact]
    public void TestOrderBySingleColumnWithDirection()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .OrderBy("AGE", OrderDirection.DESC);

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES ORDER BY AGE DESC", sql);
    }

    [Fact]
    public void TestFilterByNullOperators_IsNull()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("AGE", NullOperators.IS_NULL);

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE IS NULL", sql);
    }

    [Fact]
    public void TestFilterByNullOperators_IsNotNull()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("AGE", NullOperators.IS_NOT_NULL);

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE IS NOT NULL", sql);
    }

    [Fact]
    public void TestFilterByRangeOperators_Between()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("AGE", RangeOperators.BETWEEN, "25", "35");

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE BETWEEN 25 AND 35", sql);
    }

    [Fact]
    public void TestFilterByRangeOperators_NotBetween()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("AGE", RangeOperators.NOT_BETWEEN, "25", "35");

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE NOT BETWEEN 25 AND 35", sql);
    }

    [Fact]
    public void TestFilterByStringOperators_Like()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("NAME", StringOperators.LIKE, "'A%'");

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE NAME LIKE 'A%'", sql);
    }

    [Fact]
    public void TestFilterByStringOperators_ILike()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("NAME", StringOperators.ILIKE, "'a%'");

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE NAME ILIKE 'a%'", sql);
    }

    [Fact]
    public void TestFilterByRawStringOperator()
    {
        // e.g. FilterBy("AGE", ">=", "30")
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("AGE", ">=", "30");

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE >= 30", sql);
    }

    [Fact]
    public void TestOrFilterByComparison()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("AGE", ComparisonOperators.GREATER_THAN, "30")
            .OrFilterBy("CITY", ComparisonOperators.EQUAL, "'New York'");

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE > 30 OR CITY = 'New York'", sql);
    }

    [Fact]
    public void TestOrFilterByStringOperator()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("NAME", ComparisonOperators.EQUAL, "'Alice'")
            .OrFilterBy("NAME", StringOperators.LIKE, "'B%'");

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE NAME = 'Alice' OR NAME LIKE 'B%'", sql);
    }

    [Fact]
    public void TestOrFilterByRangeOperators_Between()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("AGE", ComparisonOperators.LESS_THAN, "18")
            .OrFilterBy("AGE", RangeOperators.BETWEEN, "30", "40");

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE < 18 OR AGE BETWEEN 30 AND 40", sql);
    }

    [Fact]
    public void TestOrFilterByNullOperators()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("CITY", ComparisonOperators.EQUAL, "'Boston'")
            .OrFilterBy("CITY", NullOperators.IS_NULL);

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE CITY = 'Boston' OR CITY IS NULL", sql);
    }

    [Fact]
    public void TestOrFilterByInclusionOperators()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("AGE", InclusionOperators.IN, new List<string> { "25", "26" })
            .OrFilterBy("AGE", InclusionOperators.IN, new List<string> { "30", "31" });

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE IN (25, 26) OR AGE IN (30, 31)", sql);
    }

    [Fact]
    public void TestGroupByMultipleColumns()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .Columns("CITY", "DEPARTMENT_ID", "COUNT(ID) AS EMP_COUNT")
            .GroupBy("CITY", "DEPARTMENT_ID");

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT CITY, DEPARTMENT_ID, COUNT(ID) AS EMP_COUNT FROM EMPLOYEES GROUP BY CITY, DEPARTMENT_ID",
            sql);
    }

    [Fact]
    public void TestMultipleJoins_CrossAndLeft()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .Join(JoinOperators.CROSS_JOIN, "DEPARTMENTS", "EMPLOYEES.DEPARTMENT_ID = DEPARTMENTS.ID")
            .Join(JoinOperators.LEFT_JOIN, "DEPARTMENTS D2", "EMPLOYEES.DEPARTMENT_ID = D2.ID");

        var sql = qb.GenerateSql();
        var expected =
            "SELECT * FROM EMPLOYEES CROSS JOIN DEPARTMENTS ON EMPLOYEES.DEPARTMENT_ID = DEPARTMENTS.ID LEFT JOIN DEPARTMENTS D2 ON EMPLOYEES.DEPARTMENT_ID = D2.ID";
        Assert.Equal(expected, sql);
    }

    [Theory]
    [InlineData((ComparisonOperators)999)]
    public void TestInvalidComparisonOperatorThrows(ComparisonOperators invalidOp)
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES");

        Assert.Throws<ArgumentOutOfRangeException>(() => qb.FilterBy("AGE", invalidOp, "30"));
    }

    [Theory]
    [InlineData((InclusionOperators)999)]
    public void TestInvalidInclusionOperatorThrows(InclusionOperators invalidOp)
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            qb.FilterBy("AGE", invalidOp, new List<string> { "25", "26" }));
    }

    [Theory]
    [InlineData((NullOperators)999)]
    public void TestInvalidNullOperatorThrows(NullOperators invalidOp)
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES");

        Assert.Throws<ArgumentOutOfRangeException>(() => qb.FilterBy("AGE", invalidOp));
    }

    [Theory]
    [InlineData((RangeOperators)999)]
    public void TestInvalidRangeOperatorThrows(RangeOperators invalidOp)
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES");

        Assert.Throws<ArgumentOutOfRangeException>(() => qb.FilterBy("AGE", invalidOp, "25", "35"));
    }

    [Theory]
    [InlineData((StringOperators)999)]
    public void TestInvalidStringOperatorThrows(StringOperators invalidOp)
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES");

        Assert.Throws<ArgumentOutOfRangeException>(() => qb.FilterBy("NAME", invalidOp, "'A%'"));
    }

    [Theory]
    [InlineData((JoinOperators)999)]
    public void TestInvalidJoinOperatorThrows(JoinOperators invalidOp)
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            qb.Join(invalidOp, "DEPARTMENTS", "EMPLOYEES.DEPARTMENT_ID = DEPARTMENTS.ID"));
    }

    [Fact]
    public void TestFullJoinAndSelfJoin()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .Join(JoinOperators.FULL_JOIN, "DEPARTMENTS", "EMPLOYEES.DEPARTMENT_ID = DEPARTMENTS.ID")
            .Join(JoinOperators.SELF_JOIN, "EMPLOYEES E2", "EMPLOYEES.ID = E2.ID");

        var sql = qb.GenerateSql();
        const string expected =
            "SELECT * FROM EMPLOYEES FULL JOIN DEPARTMENTS ON EMPLOYEES.DEPARTMENT_ID = DEPARTMENTS.ID SELF JOIN EMPLOYEES E2 ON EMPLOYEES.ID = E2.ID";
        Assert.Equal(expected, sql);
    }

    [Fact]
    public void TestOrFilterByRawStringOperator()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("AGE", ">=", "30")
            .OrFilterBy("AGE", "!=", "40");

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE >= 30 OR AGE != 40", sql);
    }

    [Fact]
    public void TestChainedColumns()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .Columns("ID")
            .Columns("NAME", "AGE");

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT ID, NAME, AGE FROM EMPLOYEES", sql);
    }

    [Fact]
    public void TestMultipleOrderByColumns()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .OrderBy("NAME", "AGE");

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES ORDER BY NAME, AGE", sql);
    }

    [Fact]
    public void TestOrderByNoColumns()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .OrderBy(); // No columns passed

        var sql = qb.GenerateSql();
        // We expect just "SELECT * FROM EMPLOYEES" with no ORDER BY clause
        Assert.Equal("SELECT * FROM EMPLOYEES", sql);
    }

    [Fact]
    public void TestGroupByNoColumns()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .GroupBy(); // no columns

        var sql = qb.GenerateSql();
        // Should not add a "GROUP BY" clause
        Assert.Equal("SELECT * FROM EMPLOYEES", sql);
    }

    [Fact]
    public void TestSetLimitZero()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .SetLimit(0);

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES", sql);
    }

    [Fact]
    public void TestComplexAndFilters()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("CITY", StringOperators.LIKE, "'New%'")
            .FilterBy("AGE", ComparisonOperators.GREATER_OR_EQUAL, "18")
            .FilterBy("DEPARTMENT_ID", InclusionOperators.IN, new List<string> { "1", "2" })
            .FilterBy("NAME", NullOperators.IS_NOT_NULL);

        var sql = qb.GenerateSql();
        Assert.Equal(
            "SELECT * FROM EMPLOYEES WHERE CITY LIKE 'New%' AND AGE >= 18 AND DEPARTMENT_ID IN (1, 2) AND NAME IS NOT NULL",
            sql
        );
    }

    [Fact]
    public void TestComplexOrFilters()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("AGE", ComparisonOperators.LESS_THAN, "18")
            .OrFilterBy("AGE", RangeOperators.BETWEEN, "30", "40")
            .OrFilterBy("NAME", StringOperators.LIKE, "'A%'")
            .OrFilterBy("NAME", NullOperators.IS_NULL);

        var sql = qb.GenerateSql();
        Assert.Equal(
            "SELECT * FROM EMPLOYEES WHERE AGE < 18 OR AGE BETWEEN 30 AND 40 OR NAME LIKE 'A%' OR NAME IS NULL",
            sql
        );
    }

    [Fact]
    public void TestFromTableDifferentCase()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees"); // lowercased, but dictionary has "EMPLOYEES"

        var sql = qb.GenerateSql();
        Assert.Equal("SELECT * FROM EMPLOYEES", sql);
    }

    [Fact]
    public void TestFilterByEmptyRawOperator()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("AGE", "", "30"); // empty operator

        var sql = qb.GenerateSql();
        // This will produce "WHERE AGE  30" which is odd, but covers the path
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE  30", sql);
    }

    [Fact]
    public void TestOrFilterByEmptyColumn()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("EMPLOYEES")
            .FilterBy("AGE", ">", "25")
            .OrFilterBy("", "=", "NULL"); // empty column name

        var sql = qb.GenerateSql();
        // Results in: "WHERE AGE > 25 OR  = NULL"
        Assert.Equal("SELECT * FROM EMPLOYEES WHERE AGE > 25 OR  = NULL", sql);
    }

    [Fact]
    public void TestUndefinedTableThrowsExceptionInGenerateFormattedSql()
    {
        var qb = new QueryBuilder(_dbInfo);
        Assert.Throws<InvalidOperationException>(() => qb.GenerateFormattedSql());
    }

    [Fact]
    public void TestInvalidTableThrowsExceptionInGenerateFormattedSql()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo).FromTable("NonExistentTable");
        Assert.Throws<InvalidOperationException>(() => qb.GenerateFormattedSql());
    }

    [Fact]
    public void TestFormattedSqlMultipleFiltersAndNoColumns()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            // First filter
            .FilterBy("AGE", ComparisonOperators.GREATER_THAN, "30")
            // Second filter (OR)
            .OrFilterBy("CITY", StringOperators.LIKE, "'New%'");

        var formattedSql = qb.GenerateFormattedSql();

        Assert.Contains("SELECT *", formattedSql);
        Assert.Contains("FROM EMPLOYEES", formattedSql);
        Assert.Contains("WHERE AGE > 30", formattedSql);
        Assert.Contains("OR CITY LIKE 'New%'", formattedSql);
    }

    [Fact]
    public void TestFormattedSqlMultipleJoins()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .Columns("EMPLOYEES.ID", "EMPLOYEES.NAME", "DEPARTMENTS.DEPT_NAME")
            .Join(JoinOperators.INNER_JOIN, "DEPARTMENTS", "EMPLOYEES.DEPARTMENT_ID = DEPARTMENTS.ID")
            .Join(JoinOperators.LEFT_JOIN, "DEPARTMENTS D2", "EMPLOYEES.DEPARTMENT_ID = D2.ID")
            .FilterBy("AGE", ComparisonOperators.GREATER_THAN, "30")
            .OrFilterBy("CITY", StringOperators.LIKE, "'New%'")
            .GroupBy("EMPLOYEES.ID", "DEPARTMENTS.ID")
            .OrderBy("EMPLOYEES.NAME", OrderDirection.ASC)
            .SetLimit(10);

        var formattedSql = qb.GenerateFormattedSql();

        Assert.Contains("SELECT EMPLOYEES.ID, EMPLOYEES.NAME, DEPARTMENTS.DEPT_NAME", formattedSql);
        Assert.Contains("FROM EMPLOYEES", formattedSql);
        Assert.Contains("INNER JOIN DEPARTMENTS ON EMPLOYEES.DEPARTMENT_ID = DEPARTMENTS.ID", formattedSql);
        Assert.Contains("LEFT JOIN DEPARTMENTS D2 ON EMPLOYEES.DEPARTMENT_ID = D2.ID", formattedSql);
        Assert.Contains("WHERE AGE > 30", formattedSql);
        Assert.Contains("OR CITY LIKE 'New%'", formattedSql);
        Assert.Contains("GROUP BY EMPLOYEES.ID, DEPARTMENTS.ID", formattedSql);
        Assert.Contains("ORDER BY EMPLOYEES.NAME ASC", formattedSql);
        Assert.Contains("LIMIT 10", formattedSql);
    }

    [Fact]
    public void TestFormattedSqlChainedColumns()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .Columns("ID")
            .Columns("NAME", "AGE")
            .FilterBy("AGE", ComparisonOperators.LESS_THAN, "50");

        var formattedSql = qb.GenerateFormattedSql();

        Assert.Contains("SELECT ID, NAME, AGE", formattedSql);
        Assert.Contains("FROM EMPLOYEES", formattedSql);
        Assert.Contains("WHERE AGE < 50", formattedSql);
    }

    [Fact]
    public void TestFormattedSqlWithLimitZero()
    {
        IQueryBuilder qb = new QueryBuilder(_dbInfo)
            .FromTable("employees")
            .Columns("ID", "NAME")
            .SetLimit(0);

        var formattedSql = qb.GenerateFormattedSql();

        Assert.Contains("SELECT ID, NAME", formattedSql);
        Assert.Contains("FROM EMPLOYEES", formattedSql);
        Assert.Contains("LIMIT 0", formattedSql);
    }
}
