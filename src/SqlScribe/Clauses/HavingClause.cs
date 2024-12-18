using System;
using System.Linq.Expressions;
using SqlScribe.Enums;

namespace SqlScribe.Clauses;

public abstract class BaseHavingClause
{
    public abstract AggregateFunction Function { get; }
    public abstract LambdaExpression Selector { get; }
    public abstract SqlOperator Operator { get; }
    public abstract object Value { get; }
}

public class HavingClause<TEntity, TValue> : BaseHavingClause
    where TValue : notnull
{
    private readonly Expression<Func<TEntity, TValue>> _selector;
    private readonly TValue _value;

    public HavingClause(AggregateFunction function, Expression<Func<TEntity, TValue>> selector,
        SqlOperator sqlOperator, TValue value)
    {
        _selector = selector;
        _value = value;
        Function = function;
        Operator = sqlOperator;
    }

    public override AggregateFunction Function { get; }
    public override LambdaExpression Selector => _selector;
    public override SqlOperator Operator { get; }
    public override object Value => _value;
}