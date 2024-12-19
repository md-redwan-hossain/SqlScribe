using System.Data.Common;
using BenchmarkDotNet.Attributes;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SqlKata.Compilers;
using SqlKata.Execution;
using SqlScribe.Enums;
using SqlScribe.ExampleScaffolder.Domain;
using SqlScribe.ExampleScaffolder.Persistence;
using SqlScribe.Factories;

namespace SqlScribe.BenchMark;

public class DbQueryBenchmark
{
    private BookDbContext? _dbContext;
    private DbConnection? _dbConnection;
    private SqliteCompiler? _compiler;
    private SqlQueryBuilderFactory? _sqlQueryBuilderFactory;

    [GlobalSetup]
    public void Setup()
    {
        BenchmarkDbSetup.InitializeDatabase();
        var dbContextOptions = BenchmarkDbSetup.CreateDbContextOptions();
        _dbContext = new BookDbContext(dbContextOptions);
        _dbConnection = BenchmarkDbSetup.CreateDbConnectionFactory().CreateConnectionAsync().Result;
        _sqlQueryBuilderFactory = BenchmarkDbSetup.CreateSqlQueryBuilderFactory();
        _compiler = new SqliteCompiler();
    }

    [Benchmark]
    public async Task<ICollection<Book>> GetWithEntityFramework()
    {
        if (_dbContext is null)
        {
            return [];
        }

        return await _dbContext.Books.Where(x => x.Id == 1)
            .AsNoTracking()
            .ToListAsync();
    }

    [Benchmark]
    public async Task<ICollection<Book>> GetWithSqlScribe()
    {
        if (_dbConnection is null || _sqlQueryBuilderFactory is null)
        {
            return [];
        }

        var query = _sqlQueryBuilderFactory.CreateSqlQueryBuilder<Book>()
            .SelectAll()
            .Where(x => x.Id, SqlOperator.Equal, 1)
            .Build();

        return (await _dbConnection.QueryAsync<Book>(query.Sql, query.Parameters)).ToList();
    }

    [Benchmark]
    public async Task<ICollection<Book>> GetWithSqlKata()
    {
        if (_dbConnection is null || _compiler is null)
        {
            return [];
        }

        var db = new QueryFactory(_dbConnection, _compiler);
        return (await db.Query("books").Where("id", 1).GetAsync<Book>()).ToList();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _dbContext?.Dispose();
        _dbConnection?.Dispose();
    }
}