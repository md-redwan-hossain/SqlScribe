using System;
using System.Linq.Expressions;

namespace SqlScribe.Clauses;

public abstract class BaseGroupByClause
{
    public abstract LambdaExpression Selector { get; }
}

public class GroupByClause<TEntity, TValue> : BaseGroupByClause
{
    private readonly Expression<Func<TEntity, TValue>> _selector;

    public GroupByClause(Expression<Func<TEntity, TValue>> selector)
    {
        _selector = selector;
    }

    public override LambdaExpression Selector => _selector;
}