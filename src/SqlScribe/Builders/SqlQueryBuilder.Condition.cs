using System.Linq.Expressions;
using System.Text;
using SqlScribe.Clauses;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder
{
    public SqlQueryBuilder WhereNotIn<TEntity, TValue>(Expression<Func<TEntity, TValue>> selector,
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

    public SqlQueryBuilder WhereIn<TEntity, TValue>(Expression<Func<TEntity, TValue>> selector, params TValue[] values)
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

    public SqlQueryBuilder WhereBetween<TEntity, TValue>(Expression<Func<TEntity, TValue>> selector, TValue leftValue,
        TValue rightValue)
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyName = ExtractPropertyName(selector);
        var columnName = ConvertName(propertyName, _namingConvention);

        var leftParameter = AddParameter(leftValue);
        var rightParameter = AddParameter(rightValue);

        _whereQueue.Enqueue($" {tableName}.{columnName} BETWEEN {leftParameter} AND ({rightParameter} ");

        return this;
    }

    public SqlQueryBuilder WhereNotBetween<TEntity, TValue>(Expression<Func<TEntity, TValue>> selector,
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

    public SqlQueryBuilder AndWhere<TLeftOperandEntity, TLeftOperandValue, TRightOperandEntity, TRightOperandValue>(
        WhereClause<TLeftOperandEntity, TLeftOperandValue> leftOperand,
        WhereClause<TRightOperandEntity, TRightOperandValue> rightOperand, bool wrapWithParenthesis)
        where TLeftOperandValue : notnull
        where TRightOperandValue : notnull
    {
        var left = new StringBuilder();

        left.Append($"{GetTableName(typeof(TLeftOperandEntity))}.{ExtractPropertyName(leftOperand.Selector)}")
            .Append($" {GetMappedOperator(leftOperand.Operator)} ")
            .Append($"{AddParameter(leftOperand.Value)}");

        var right = new StringBuilder();

        right.Append($"{GetTableName(typeof(TRightOperandEntity))}.{ExtractPropertyName(rightOperand.Selector)}")
            .Append($" {GetMappedOperator(rightOperand.Operator)} ")
            .Append($"{AddParameter(rightOperand.Value)}");

        _whereQueue.Enqueue(wrapWithParenthesis ? $"({left} AND {right})" : $"{left} AND {right}");

        return this;
    }

    public SqlQueryBuilder OrWhere<TLeftOperandEntity, TLeftOperandValue, TRightOperandEntity, TRightOperandValue>(
        WhereClause<TLeftOperandEntity, TLeftOperandValue> leftOperand,
        WhereClause<TRightOperandEntity, TRightOperandValue> rightOperand, bool wrapWithParenthesis)
        where TLeftOperandValue : notnull
        where TRightOperandValue : notnull
    {
        var left = new StringBuilder();

        left.Append($"{GetTableName(typeof(TLeftOperandEntity))}.{ExtractPropertyName(leftOperand.Selector)}")
            .Append($" {GetMappedOperator(leftOperand.Operator)} ")
            .Append($"{AddParameter(leftOperand.Value)}");

        var right = new StringBuilder();

        right.Append($"{GetTableName(typeof(TRightOperandEntity))}.{ExtractPropertyName(rightOperand.Selector)}")
            .Append($" {GetMappedOperator(rightOperand.Operator)} ")
            .Append($"{AddParameter(rightOperand.Value)}");

        _whereQueue.Enqueue(wrapWithParenthesis ? $"({left} OR {right})" : $"{left} OR {right}");

        return this;
    }

    public SqlQueryBuilder OrWhere<TEntity>(params BaseWhereClause[] clauses)
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

    public SqlQueryBuilder AndWhere<TEntity>(params BaseWhereClause[] clauses)
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

    public SqlQueryBuilder Where<TEntity, TValue>(WhereClause<TEntity, TValue> clause)
        where TValue : notnull
    {
        var tableName = GetTableName(typeof(TEntity));
        var propertyName = ExtractPropertyName(clause.Selector);
        var columnName = ConvertName(propertyName, _namingConvention);

        var paramName = AddParameter(clause.Value);
        _whereQueue.Enqueue($"{tableName}.{columnName} {GetMappedOperator(clause.Operator)} {paramName}");
        return this;
    }

    public SqlQueryBuilder Having<TEntity, TValue>(HavingClause<TEntity, TValue> clause)
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


    public SqlQueryBuilder OrHaving<TEntity>(params BaseHavingClause[] clauses)
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

    public SqlQueryBuilder AndHaving<TEntity>(params BaseHavingClause[] clauses)
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