using System;
using System.Linq.Expressions;
using SqlScribe.Clauses;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder<TEntity>
{
    public SqlQueryBuilder<TEntity> SelectAll()
    {
        _hasSelectAllStatement = true;
        return this;
    }

    public SqlQueryBuilder<TEntity> Select<TValue>(Expression<Func<TEntity, TValue>> selector)
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyNames = ExtractPropertyNames(selector);

        foreach (var item in propertyNames)
        {
            var columnName = ConvertName(item, _namingConvention);
            _selectQueue.Enqueue($"{tableName}.{columnName}");
        }

        return this;
    }

    public SqlQueryBuilder<TEntity> Select(params BaseSelectClause[] clauses)
    {
        var tableName = GetTableName(typeof(TEntity));
        foreach (var item in clauses)
        {
            var propertyNames = ExtractPropertyNames(item.Selector);

            foreach (var innerItem in propertyNames)
            {
                var columnName = ConvertName(innerItem, _namingConvention);
                _selectQueue.Enqueue($"{tableName}.{columnName}");
            }
        }

        return this;
    }

    public SqlQueryBuilder<TEntity> Select<TSourceEntity, TDestinationEntity, TValue>(
        Expression<Func<TSourceEntity, TValue>> sourceSelector,
        Expression<Func<TDestinationEntity, TValue>> destinationSelector)
    {
        var sourceTableName = GetTableName(typeof(TSourceEntity));
        var sourceColumnName = ConvertName(ExtractPropertyName(sourceSelector), _namingConvention);
        var destinationColumnName = ConvertName(ExtractPropertyName(destinationSelector), _namingConvention);

        _selectQueue.Enqueue($"{sourceTableName}.{sourceColumnName} AS {destinationColumnName}");
        return this;
    }
}