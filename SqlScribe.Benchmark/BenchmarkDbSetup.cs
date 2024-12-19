using Microsoft.EntityFrameworkCore;
using SqlScribe.Enums;
using SqlScribe.ExampleScaffolder.Persistence;
using SqlScribe.Factories;

namespace SqlScribe.BenchMark;

public static class BenchmarkDbSetup
{
    private const string ConnectionString = "DataSource=db.sqlite3;Cache=Shared;";

    public static DbContextOptions<BookDbContext> CreateDbContextOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BookDbContext>();
        optionsBuilder.UseSqlite(ConnectionString);
        return optionsBuilder.Options;
    }

    public static void InitializeDatabase()
    {
        var options = CreateDbContextOptions();
        using var dbContext = new BookDbContext(options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    public static IDbConnectionFactory CreateDbConnectionFactory()
    {
        return new SqliteConnectionFactory(ConnectionString);
    }

    public static SqlQueryBuilderFactory CreateSqlQueryBuilderFactory()
    {
        return new SqlQueryBuilderFactory(DatabaseVendor.SqLite, SqlNamingConvention.LowerSnakeCase, true);
    }
}