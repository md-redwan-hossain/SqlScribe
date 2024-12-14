using Dapper;
using Dumpify;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SharpOutcome;
using SharpOutcome.Helpers;
using SharpOutcome.Helpers.Contracts;
using SharpOutcome.Helpers.Enums;
using SqlScribe.Clauses;
using SqlScribe.Enums;
using SqlScribe.Factories;
using SqlScribe.HttpApiExample.Data;
using SqlScribe.HttpApiExample.DataTransferObjects;

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

    public async Task<IList<Book>> GetAllAsync()
    {
        await using var conn = await _dbConnectionFactory.CreateConnectionAsync();
        var qb = _sqlQueryBuilderFactory.CreateSqlQueryBuilder();

        var query = qb
            .SelectAll()
            .Build<Book>();

        return (await conn.QueryAsync<Book>(query.Sql, query.Parameters)).ToList();
    }

    public async Task<IList<BookSlimResponse>> GetAllByPriceRangeAsync(decimal lowerBound, decimal upperBound,
        CancellationToken ct)
    {
        await using var conn = await _dbConnectionFactory.CreateConnectionAsync(ct);
        var qb = _sqlQueryBuilderFactory.CreateSqlQueryBuilder();

        var query = qb
            .Select<Book>(
                new SelectClause<Book, string>(x => x.Title),
                new SelectClause<Book, decimal>(x => x.Price)
            )
            .GroupBy<Book>(
                new GroupByClause<Book, string>(x => x.Title),
                new GroupByClause<Book, decimal>(x => x.Price)
            )
            .AndHaving<Book>(
                new HavingClause<Book, decimal>(AggregateFunction.Min, x => x.Price,
                    SqlOperator.GreaterThanOrEqual, lowerBound),
                new HavingClause<Book, decimal>(AggregateFunction.Max, x => x.Price,
                    SqlOperator.LessThanOrEqual, upperBound)
            )
            .OrderByAsc<Book, decimal>(x => x.Price)
            .Build<Book>();

        query.Dump();

        return (await conn.QueryAsync<BookSlimResponse>(new CommandDefinition(query.Sql, query.Parameters,
            cancellationToken: ct))).ToList();
    }

    public async Task<ValueOutcome<Book, IBadOutcome>> GetOneAsync(int id)
    {
        await using var conn = await _dbConnectionFactory.CreateConnectionAsync();
        var qb = _sqlQueryBuilderFactory.CreateSqlQueryBuilder();

        var query = qb
            .SelectAll()
            .Where(new WhereClause<Book, int>(x => x.Id, SqlOperator.Equal, id))
            .Build<Book>();

        var entity = await conn.QueryFirstOrDefaultAsync<Book>(query.Sql, query.Parameters);

        if (entity is null)
        {
            return new BadOutcome(BadOutcomeTag.NotFound);
        }

        return entity;
    }

    public async Task<ValueOutcome<IGoodOutcome, IBadOutcome>> RemoveAsync(int id)
    {
        await using var conn = await _dbConnectionFactory.CreateConnectionAsync();
        var qb = _sqlQueryBuilderFactory.CreateSqlQueryBuilder();

        var query = qb
            .Delete()
            .Where(new WhereClause<Book, int>(x => x.Id, SqlOperator.Equal, id))
            .Build<Book>();

        var rowsAffected = await conn.ExecuteAsync(query.Sql, query.Parameters);

        if (rowsAffected > 0)
        {
            return new GoodOutcome(GoodOutcomeTag.Deleted);
        }

        return new BadOutcome(BadOutcomeTag.NotFound);
    }
}