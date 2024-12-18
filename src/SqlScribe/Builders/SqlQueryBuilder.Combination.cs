using System;
using System.Linq.Expressions;
using SqlScribe.Clauses;
using SqlScribe.Enums;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder<TEntity>
{
    public SqlQueryBuilder<TEntity> GroupBy<TValue>(Expression<Func<TEntity, TValue>> selector)
    {
        var tableName = GetTableName(typeof(TEntity));

        var propertyNames = ExtractPropertyNames(selector);

        foreach (var item in propertyNames)
        {
            var columnName = ConvertName(item, _namingConvention);
            _groupByQueue.Enqueue($" {tableName}.{columnName} ");
        }

        return this;
    }

    public SqlQueryBuilder<TEntity> GroupBy(params BaseGroupByClause[] clauses)
    {
        var tableName = GetTableName(typeof(TEntity));
        foreach (var item in clauses)
        {
            var propertyNames = ExtractPropertyNames(item.Selector);

            foreach (var innerItem in propertyNames)
            {
                var columnName = ConvertName(innerItem, _namingConvention);
                _groupByQueue.Enqueue($"{tableName}.{columnName}");
            }
        }

        return this;
    }

    public SqlQueryBuilder<TEntity> Join<TJoinedEntity, TEntityValue, TJoinedEntityValue>(SqlJoinType sqlJoinType,
        Expression<Func<TEntity, TEntityValue>> entityKey,
        Expression<Func<TJoinedEntity, TJoinedEntityValue>> joinedEntityKey)
    {
        var joinTypeString = sqlJoinType switch
        {
            SqlJoinType.Inner => "INNER JOIN",
            SqlJoinType.Left => "LEFT JOIN",
            SqlJoinType.Right => "RIGHT JOIN",
            SqlJoinType.Full => "FULL JOIN",
            _ => throw new ArgumentException("Invalid join type")
        };

        var fromTable = GetTableName(typeof(TEntity));
        var toTable = GetTableName(typeof(TJoinedEntity));

        var fromColumn =
            ConvertName(ExtractPropertyName(entityKey), _namingConvention);
        var toColumn = ConvertName(ExtractPropertyName(joinedEntityKey), _namingConvention);

        _joinQueue.Enqueue($" {joinTypeString} {toTable} ON {fromTable}.{fromColumn} = {toTable}.{toColumn} ");

        return this;
    }
}