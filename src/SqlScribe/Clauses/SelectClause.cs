using System.Linq.Expressions;

namespace SqlScribe.Clauses;

// public record SelectClause<TEntity, TValue>(Expression<Func<TEntity, TValue>> Selector, string? Alias = null);
public abstract class BaseSelectClause
{
    public abstract LambdaExpression Selector { get; }
    public abstract string? Alias { get; }
}

public class SelectClause<TEntity, TValue> : BaseSelectClause
{
    private readonly Expression<Func<TEntity, TValue>> _selector;

    public SelectClause(Expression<Func<TEntity, TValue>> selector, string? alias = null)
    {
        _selector = selector;
        Alias = alias;
    }

    public override LambdaExpression Selector => _selector;
    public override string? Alias { get; }
}