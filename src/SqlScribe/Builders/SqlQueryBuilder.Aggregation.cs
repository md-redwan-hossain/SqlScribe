using System.Linq.Expressions;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder<TEntity>
{
    public SqlQueryBuilder<TEntity> Count<TValue>(Expression<Func<TEntity, TValue>> selector, string? alias = null)
    {
        var tableName = GetTableName(typeof(TEntity));
        var columnName = ConvertName(ExtractPropertyName(selector), _namingConvention);

        var final = string.IsNullOrEmpty(alias)
            ? $" COUNT({tableName}.{columnName}) "
            : $" COUNT({tableName}.{columnName}) as {DelimitString(alias)} ";

        _aggregateQueue.Enqueue(final);
        return this;
    }

    public SqlQueryBuilder<TEntity> Max<TValue>(Expression<Func<TEntity, TValue>> selector, string? alias = null)
    {
        var tableName = GetTableName(typeof(TEntity));
        var columnName = ConvertName(ExtractPropertyName(selector), _namingConvention);

        var final = string.IsNullOrEmpty(alias)
            ? $" MAX({tableName}.{columnName}) "
            : $" MAX({tableName}.{columnName}) as {DelimitString(alias)} ";

        _aggregateQueue.Enqueue(final);
        return this;
    }

    public SqlQueryBuilder<TEntity> Sum<TValue>(Expression<Func<TEntity, TValue>> selector, string? alias = null,
        bool useCoalesce = false, TValue? coalesceValue = default)
    {
        var tableName = GetTableName(typeof(TEntity));
        var columnName = ConvertName(ExtractPropertyName(selector), _namingConvention);

        var transform = useCoalesce && coalesceValue is not null
            ? $" COALESCE(SUM({tableName}.{columnName}), {AddParameter(coalesceValue)}) "
            : $" SUM({tableName}.{columnName}) ";

        if (string.IsNullOrEmpty(alias) is false)
        {
            transform = $"{transform} as {DelimitString(alias)}";
        }

        _aggregateQueue.Enqueue(transform);
        return this;
    }
}