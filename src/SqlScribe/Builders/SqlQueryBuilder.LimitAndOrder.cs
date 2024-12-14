using System.Linq.Expressions;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder
{
    public SqlQueryBuilder OrderByAsc<TEntity, TValue>(Expression<Func<TEntity, TValue>> selector)
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyName = ExtractPropertyName(selector);
        var columnName = ConvertName(propertyName, _namingConvention);
        _orderByQueue.Enqueue($" {tableName}.{columnName} ASC ");
        return this;
    }

    public SqlQueryBuilder OrderByDesc<TEntity, TValue>(Expression<Func<TEntity, TValue>> selector)
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyName = ExtractPropertyName(selector);
        var columnName = ConvertName(propertyName, _namingConvention);
        _orderByQueue.Enqueue($" {tableName}.{columnName} DESC ");
        return this;
    }

    public SqlQueryBuilder SetLimit(int limit)
    {
        if (limit <= 0)
        {
            limit = 1;
        }

        _limit = limit;
        return this;
    }

    public SqlQueryBuilder SetPage(int page)
    {
        if (page <= 0)
        {
            page = 1;
        }

        _page = page;
        return this;
    }
}