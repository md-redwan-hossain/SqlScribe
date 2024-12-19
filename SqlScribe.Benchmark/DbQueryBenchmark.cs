using System.Data.Common;
using BenchmarkDotNet.Attributes;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SqlKata.Compilers;
using SqlKata.Execution;
using SqlScribe.ExampleScaffolder.DataTransferObjects;
using SqlScribe.ExampleScaffolder.Domain;
using SqlScribe.ExampleScaffolder.Persistence;
using SqlScribe.Factories;

namespace SqlScribe.BenchMark;

[MemoryDiagnoser]
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
    public async Task<ICollection<BookResponse>> GetWithEntityFramework()
    {
        if (_dbContext is null)
        {
            return [];
        }

        var query = from book in _dbContext.Books
            join authorTemp in _dbContext.Authors
                on book.AuthorId equals authorTemp.Id into authorLeftJoin
            from author in authorLeftJoin.DefaultIfEmpty()
            select new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                Genre = book.Genre,
                AuthorFirstName = author.FirstName,
                AuthorLastName = author.LastName,
                Price = book.Price
            };

        return await query
            .AsNoTracking()
            .ToListAsync();
    }

    [Benchmark]
    public async Task<ICollection<BookResponse>> GetWithSqlScribe()
    {
        if (_dbConnection is null || _sqlQueryBuilderFactory is null)
        {
            return [];
        }

        var query = _sqlQueryBuilderFactory.CreateSqlQueryBuilder<Book>()
            .LeftJoin<Author, int?>(entity => entity.AuthorId, joined => joined.Id)
            .Select(entity => new { entity.Id, entity.Title, entity.Genre, entity.Isbn, entity.Price })
            .SelectWithMapping<Author, BookResponse, string?>(x => x.FirstName, y => y.AuthorFirstName)
            .SelectWithMapping<Author, BookResponse, string?>(x => x.LastName, y => y.AuthorLastName)
            .Build();

        return (await _dbConnection.QueryAsync<BookResponse>(query.Sql, query.Parameters)).ToList();
    }

    [Benchmark]
    public async Task<ICollection<Book>> GetWithSqlKata()
    {
        if (_dbConnection is null || _compiler is null)
        {
            return [];
        }

        var db = new QueryFactory(_dbConnection, _compiler);
        return (await db.Query("books").Join("authors", "authors.id", "books.author_id")
                .Select("books.id", "books.title", "books.genre", "books.isbn", "books.price")
                .SelectRaw("authors.first_name AS author_first_name")
                .SelectRaw("authors.last_name AS author_last_name")
                .GetAsync<Book>())
            .ToList();
    }

    [Benchmark]
    public async Task<ICollection<BookResponse>> GetWithDapper()
    {
        if (_dbConnection is null)
        {
            return [];
        }

        const string query = """
                             SELECT
                               "books"."id",
                               "books"."title",
                               "books"."genre",
                               "books"."isbn",
                               "books"."price",
                               authors.first_name AS author_first_name,
                               authors.last_name AS author_last_name
                             FROM
                               "books"
                               INNER JOIN "authors" ON "authors"."id" = "books"."author_id" 
                             """;

        return (await _dbConnection.QueryAsync<BookResponse>(query)).ToList();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _dbContext?.Dispose();
        _dbConnection?.Dispose();
    }
}