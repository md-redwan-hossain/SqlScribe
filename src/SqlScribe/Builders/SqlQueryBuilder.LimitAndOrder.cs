using System.Linq.Expressions;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder<TEntity>
{
    public SqlQueryBuilder<TEntity> OrderByAsc<TValue>(Expression<Func<TEntity, TValue>> selector)
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyNames = ExtractPropertyNames(selector);

        foreach (var item in propertyNames)
        {
            var columnName = ConvertName(item, _namingConvention);
            _orderByQueue.Enqueue($" {tableName}.{columnName} ASC ");
        }

        return this;
    }

    public SqlQueryBuilder<TEntity> OrderByDesc<TValue>(Expression<Func<TEntity, TValue>> selector)
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyNames = ExtractPropertyNames(selector);

        foreach (var item in propertyNames)
        {
            var columnName = ConvertName(item, _namingConvention);
            _orderByQueue.Enqueue($" {tableName}.{columnName} DESC ");
        }

        return this;
    }

    public SqlQueryBuilder<TEntity> SetLimit(int limit)
    {
        if (limit <= 0)
        {
            limit = 1;
        }

        _limit = limit;
        return this;
    }

    public SqlQueryBuilder<TEntity> SetPage(int page)
    {
        if (page <= 0)
        {
            page = 1;
        }

        _page = page;
        return this;
    }
}