using System.Linq.Expressions;
using SqlScribe.Enums;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder
{
    public SqlQueryBuilder GroupBy<TEntity, TValue>(Expression<Func<TEntity, TValue>> selector)
    {
        var tableName = GetTableName(typeof(TEntity));
        var columnName = ConvertName(ExtractPropertyName(selector), _namingConvention);
        _groupByQueue.Enqueue($" {tableName}.{columnName} ");
        return this;
    }

    public SqlQueryBuilder Join<TPrimary, TJoined, TParenTValue, TChildValue>(SqlJoinType sqlJoinType,
        Expression<Func<TPrimary, TParenTValue>> fromKey, Expression<Func<TJoined, TChildValue>> toKey)
    {
        var joinTypeString = sqlJoinType switch
        {
            SqlJoinType.Inner => "INNER JOIN",
            SqlJoinType.Left => "LEFT JOIN",
            SqlJoinType.Right => "RIGHT JOIN",
            SqlJoinType.Full => "FULL JOIN",
            _ => throw new ArgumentException("Invalid join type")
        };

        var fromTable = GetTableName(typeof(TPrimary));
        var toTable = GetTableName(typeof(TJoined));

        var fromColumn =
            ConvertName(ExtractPropertyName(fromKey), _namingConvention);
        var toColumn = ConvertName(ExtractPropertyName(toKey), _namingConvention);

        _joinQueue.Enqueue($" {joinTypeString} {toTable} ON {fromTable}.{fromColumn} = {toTable}.{toColumn} ");

        return this;
    }
}