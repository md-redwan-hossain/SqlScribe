using System;
using System.Linq.Expressions;

namespace SqlScribe.Clauses;

public abstract class BaseSelectClause
{
    public abstract LambdaExpression Selector { get; }
}

public class SelectClause<TEntity, TValue> : BaseSelectClause
{
    private readonly Expression<Func<TEntity, TValue>> _selector;

    public SelectClause(Expression<Func<TEntity, TValue>> selector)
    {
        _selector = selector;
    }

    public override LambdaExpression Selector => _selector;
}