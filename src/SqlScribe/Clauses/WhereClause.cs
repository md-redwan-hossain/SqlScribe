using System;
using System.Linq.Expressions;
using SqlScribe.Enums;

namespace SqlScribe.Clauses;

public abstract class BaseWhereClause
{
    public abstract LambdaExpression Selector { get; }
    public abstract SqlOperator Operator { get; }
    public abstract object Value { get; }
}

public class WhereClause<TEntity, TValue> : BaseWhereClause
    where TValue : notnull
{
    private readonly Expression<Func<TEntity, TValue>> _selector;
    private readonly TValue _value;

    public WhereClause(Expression<Func<TEntity, TValue>> selector, SqlOperator sqlOperator, TValue value)
    {
        _selector = selector;
        Operator = sqlOperator;
        _value = value;
    }

    public override LambdaExpression Selector => _selector;
    public override SqlOperator Operator { get; }
    public override object Value => _value;
}