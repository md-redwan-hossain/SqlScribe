using Dapper;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SharpOutcome;
using SharpOutcome.Helpers;
using SharpOutcome.Helpers.Contracts;
using SharpOutcome.Helpers.Enums;
using SqlScribe.Enums;
using SqlScribe.ExampleScaffolder.DataTransferObjects;
using SqlScribe.ExampleScaffolder.Domain;
using SqlScribe.ExampleScaffolder.Persistence;
using SqlScribe.Factories;

namespace SqlScribe.HttpApiExample.Services;

public class BookService : IBookService
{
    private readonly SqlQueryBuilderFactory _sqlQueryBuilderFactory;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly BookDbContext _bookDbContext;

    public BookService(BookDbContext bookDbContext, IDbConnectionFactory dbConnectionFactory,
        SqlQueryBuilderFactory sqlQueryBuilderFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _sqlQueryBuilderFactory = sqlQueryBuilderFactory;
        _bookDbContext = bookDbContext;
    }

    public async Task<ValueOutcome<Book, IBadOutcome>> CreateAsync(BookRequest dto)
    {
        try
        {
            var duplicateIsbn = await _bookDbContext.Books
                .Where(x => x.Isbn == dto.Isbn)
                .FirstOrDefaultAsync();

            if (duplicateIsbn is not null)
            {
                return new BadOutcome(BadOutcomeTag.Conflict, $"Duplicate isbn: {dto.Isbn}");
            }

            var entity = await dto.BuildAdapter().AdaptToTypeAsync<Book>();
            await _bookDbContext.Books.AddAsync(entity);
            await _bookDbContext.SaveChangesAsync();
            return entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new BadOutcome(BadOutcomeTag.Unexpected);
        }
    }

    public async Task<ValueOutcome<Book, IBadOutcome>> UpdateAsync(int id, BookRequest dto)
    {
        try
        {
            var entityToUpdate = await _bookDbContext.Books.FirstOrDefaultAsync(x => x.Id == id);
            if (entityToUpdate is null) return new BadOutcome(BadOutcomeTag.NotFound);

            await dto.BuildAdapter().AdaptToAsync(entityToUpdate);
            _bookDbContext.Books.Attach(entityToUpdate);
            _bookDbContext.Entry(entityToUpdate).State = EntityState.Modified;
            await _bookDbContext.SaveChangesAsync();
            return entityToUpdate;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new BadOutcome(BadOutcomeTag.Unexpected);
        }
    }

    public async Task<IEnumerable<BookResponse>> GetAllAsync()
    {
        await using var conn = await _dbConnectionFactory.CreateConnectionAsync();

        var query = _sqlQueryBuilderFactory.CreateSqlQueryBuilder<Book>()
            .LeftJoin<Author, int?>(entity => entity.AuthorId, joined => joined.Id)
            .Select(entity => new { entity.Id, entity.Title, entity.Genre, entity.Isbn, entity.Price })
            .SelectWithMapping<Author, BookResponse, string?>(x => x.FirstName, y => y.AuthorFirstName)
            .SelectWithMapping<Author, BookResponse, string?>(x => x.LastName, y => y.AuthorLastName)
            .Build();

        return await conn.QueryAsync<BookResponse>(query.Sql, query.Parameters);
    }

    public async Task<IEnumerable<BookSlimResponse>> GetAllByPriceRangeAsync(decimal lowerBound, decimal upperBound,
        CancellationToken ct)
    {
        await using var conn = await _dbConnectionFactory.CreateConnectionAsync(ct);

        var query = _sqlQueryBuilderFactory.CreateSqlQueryBuilder<Book>()
            .Select(x => new { x.Title, x.Price })
            .GroupBy(x => new { x.Title, x.Price })
            .Having(AggregateFunction.Min, x => x.Price, SqlOperator.GreaterThanOrEqual, lowerBound)
            .And(SqlKeywordLocation.HavingClause)
            .Having(AggregateFunction.Max, x => x.Price, SqlOperator.LessThanOrEqual, upperBound)
            .OrderByAsc(x => x.Price)
            .Build();

        return await conn.QueryAsync<BookSlimResponse>(new CommandDefinition(query.Sql, query.Parameters,
            cancellationToken: ct));
    }

    public async Task<ValueOutcome<BookResponse, IBadOutcome>> GetOneAsync(int id)
    {
        await using var conn = await _dbConnectionFactory.CreateConnectionAsync();

        var query = _sqlQueryBuilderFactory.CreateSqlQueryBuilder<Book>()
            .SelectAll()
            .Where(x => x.Id, SqlOperator.Equal, id)
            .Build();

        var entity = await conn.QueryFirstOrDefaultAsync<BookResponse>(query.Sql, query.Parameters);

        if (entity is null)
        {
            return new BadOutcome(BadOutcomeTag.NotFound);
        }

        return entity;
    }

    public async Task<ValueOutcome<IGoodOutcome, IBadOutcome>> RemoveAsync(int id)
    {
        await using var conn = await _dbConnectionFactory.CreateConnectionAsync();

        var query = _sqlQueryBuilderFactory.CreateSqlQueryBuilder<Book>()
            .Delete()
            .Where(x => x.Id, SqlOperator.Equal, id)
            .Build();

        var rowsAffected = await conn.ExecuteAsync(query.Sql, query.Parameters);

        if (rowsAffected > 0)
        {
            return new GoodOutcome(GoodOutcomeTag.Deleted);
        }

        return new BadOutcome(BadOutcomeTag.NotFound);
    }
}