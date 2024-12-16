using System.Linq.Expressions;
using System.Text;
using SqlScribe.Clauses;
using SqlScribe.Enums;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder<TEntity>
{
    public SqlQueryBuilder<TEntity> WhereNotIn<TValue>(Expression<Func<TEntity, TValue>> selector,
        params TValue[] values)
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyName = ExtractPropertyName(selector);
        var columnName = ConvertName(propertyName, _namingConvention);

        var parameters = new StringBuilder();

        var upperBound = values.Length;
        var counter = 0;

        foreach (var elem in values)
        {
            counter += 1;
            var paramName = AddParameter(elem);
            parameters.Append(counter < upperBound ? $"{paramName}, " : paramName);
        }

        _whereQueue.Enqueue($"{tableName}.{columnName} NOT IN ({parameters})");

        return this;
    }

    public SqlQueryBuilder<TEntity> WhereIn<TValue>(Expression<Func<TEntity, TValue>> selector,
        params TValue[] values)
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyName = ExtractPropertyName(selector);
        var columnName = ConvertName(propertyName, _namingConvention);

        var parameters = new StringBuilder();

        var upperBound = values.Length;
        var counter = 0;

        foreach (var elem in values)
        {
            counter += 1;
            var paramName = AddParameter(elem);
            parameters.Append(counter < upperBound ? $"{paramName}, " : paramName);
        }

        _whereQueue.Enqueue($"{tableName}.{columnName} IN ({parameters})");

        return this;
    }

    public SqlQueryBuilder<TEntity> WhereBetween<TValue>(Expression<Func<TEntity, TValue>> selector,
        TValue leftValue, TValue rightValue)
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyName = ExtractPropertyName(selector);
        var columnName = ConvertName(propertyName, _namingConvention);

        var leftParameter = AddParameter(leftValue);
        var rightParameter = AddParameter(rightValue);

        _whereQueue.Enqueue($" {tableName}.{columnName} BETWEEN {leftParameter} AND ({rightParameter} ");

        return this;
    }

    public SqlQueryBuilder<TEntity> WhereNotBetween<TValue>(Expression<Func<TEntity, TValue>> selector,
        TValue leftValue, TValue rightValue)
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyName = ExtractPropertyName(selector);
        var columnName = ConvertName(propertyName, _namingConvention);

        var leftParameter = AddParameter(leftValue);
        var rightParameter = AddParameter(rightValue);

        _whereQueue.Enqueue($" {tableName}.{columnName} NOT BETWEEN {leftParameter} AND ({rightParameter} ");

        return this;
    }

    public SqlQueryBuilder<TEntity> OrWhere(params BaseWhereClause[] clauses)
    {
        var tableName = GetTableName(typeof(TEntity));
        var upperBound = clauses.Length;
        var counter = 0;
        foreach (var item in clauses)
        {
            counter += 1;
            var columnName = ConvertName(ExtractPropertyName(item.Selector), _namingConvention);
            _whereQueue.Enqueue(
                $" {tableName}.{columnName} {GetMappedOperator(item.Operator)} {AddParameter(item.Value)} "
            );

            if (counter < upperBound)
            {
                _whereQueue.Enqueue(" OR ");
            }
        }

        return this;
    }

    public SqlQueryBuilder<TEntity> AndWhere(params BaseWhereClause[] clauses)
    {
        var tableName = GetTableName(typeof(TEntity));
        var upperBound = clauses.Length;
        var counter = 0;
        foreach (var item in clauses)
        {
            counter += 1;
            var columnName = ConvertName(ExtractPropertyName(item.Selector), _namingConvention);
            _whereQueue.Enqueue(
                $" {tableName}.{columnName} {GetMappedOperator(item.Operator)} {AddParameter(item.Value)} "
            );

            if (counter < upperBound)
            {
                _whereQueue.Enqueue(" AND ");
            }
        }

        return this;
    }

    public SqlQueryBuilder<TEntity> Where<TValue>(Expression<Func<TEntity, TValue>> selector, SqlOperator sqlOperator,
        TValue value) where TValue : notnull
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyName = ExtractPropertyName(selector);
        var columnName = ConvertName(propertyName, _namingConvention);

        var paramName = AddParameter(value);
        _whereQueue.Enqueue($"{tableName}.{columnName} {GetMappedOperator(sqlOperator)} {paramName}");
        return this;
    }

    public SqlQueryBuilder<TEntity> Where<TValue>(WhereClause<TEntity, TValue> clause)
        where TValue : notnull
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyName = ExtractPropertyName(clause.Selector);
        var columnName = ConvertName(propertyName, _namingConvention);

        var paramName = AddParameter(clause.Value);
        _whereQueue.Enqueue($"{tableName}.{columnName} {GetMappedOperator(clause.Operator)} {paramName}");
        return this;
    }

    public SqlQueryBuilder<TEntity> Having<TValue>(AggregateFunction function,
        Expression<Func<TEntity, TValue>> selector, SqlOperator sqlOperator, TValue value)
        where TValue : notnull
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyName = ExtractPropertyName(selector);
        var columnName = ConvertName(propertyName, _namingConvention);
        var aggregateFunc = function.ToString().ToUpperInvariant();

        var paramName = AddParameter(value);
        _havingQueue.Enqueue(
            $" {aggregateFunc}({tableName}.{columnName}) {GetMappedOperator(sqlOperator)} {paramName} ");
        return this;
    }

    public SqlQueryBuilder<TEntity> Having<TValue>(HavingClause<TEntity, TValue> clause)
        where TValue : notnull
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyName = ExtractPropertyName(clause.Selector);
        var columnName = ConvertName(propertyName, _namingConvention);
        var aggregateFunc = clause.Function.ToString().ToUpperInvariant();

        var paramName = AddParameter(clause.Value);
        _havingQueue.Enqueue(
            $" {aggregateFunc}({tableName}.{columnName}) {GetMappedOperator(clause.Operator)} {paramName} ");
        return this;
    }


    public SqlQueryBuilder<TEntity> OrHaving(params BaseHavingClause[] clauses)
    {
        var tableName = GetTableName(typeof(TEntity));
        var upperBound = clauses.Length;
        var counter = 0;
        foreach (var item in clauses)
        {
            var aggregateFunc = item.Function.ToString().ToUpperInvariant();
            var paramName = AddParameter(item.Value);
            counter += 1;
            var columnName = ConvertName(ExtractPropertyName(item.Selector), _namingConvention);

            _havingQueue.Enqueue(
                $" {aggregateFunc}({tableName}.{columnName}) {GetMappedOperator(item.Operator)} {paramName} ");

            if (counter < upperBound)
            {
                _havingQueue.Enqueue(" OR ");
            }
        }

        return this;
    }

    public SqlQueryBuilder<TEntity> AndHaving(params BaseHavingClause[] clauses)
    {
        var tableName = GetTableName(typeof(TEntity));
        var upperBound = clauses.Length;
        var counter = 0;
        foreach (var item in clauses)
        {
            var aggregateFunc = item.Function.ToString().ToUpperInvariant();
            var paramName = AddParameter(item.Value);
            counter += 1;
            var columnName = ConvertName(ExtractPropertyName(item.Selector), _namingConvention);

            _havingQueue.Enqueue(
                $" {aggregateFunc}({tableName}.{columnName}) {GetMappedOperator(item.Operator)} {paramName} ");

            if (counter < upperBound)
            {
                _havingQueue.Enqueue(" AND ");
            }
        }

        return this;
    }
}