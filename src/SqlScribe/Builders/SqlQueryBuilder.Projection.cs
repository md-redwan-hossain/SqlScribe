using System.Linq.Expressions;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder
{
    public SqlQueryBuilder SelectAll()
    {
        _selectQueue.Clear();
        _selectQueue.Enqueue(" * ");
        return this;
    }

    public SqlQueryBuilder Select<TEntity, TValue>(Expression<Func<TEntity, TValue>> selector, string? alias = null)
    {
        var tableName = GetTableName(typeof(TEntity));
        var columnName = ConvertName(ExtractPropertyName(selector), _namingConvention);

        var final = string.IsNullOrEmpty(alias)
            ? $"{tableName}.{columnName}"
            : $"{tableName}.{columnName} as {DelimitString(alias)}";

        _selectQueue.Enqueue(final);
        return this;
    }

    public SqlQueryBuilder MapSelect<TSourceEntity, TResultEntity, TValue>(
        Expression<Func<TSourceEntity, TValue>> sourceSelector,
        Expression<Func<TResultEntity, TValue>> dtoSelector)
    {
        var sourceTableName = GetTableName(typeof(TSourceEntity));
        var sourceColumnName = ConvertName(ExtractPropertyName(sourceSelector), _namingConvention);
        var dtoColumnName = ConvertName(ExtractPropertyName(dtoSelector), _namingConvention);

        _selectQueue.Enqueue($"{sourceTableName}.{sourceColumnName} AS {dtoColumnName}");
        return this;
    }
}