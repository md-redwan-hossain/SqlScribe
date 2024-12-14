using System.Linq.Expressions;
using SqlScribe.Enums;


namespace SqlScribe.Clauses;

public record HavingClause<TEntity, TValue>(
    AggregateFunction Function,
    Expression<Func<TEntity, TValue>> Selector,
    SqlOperator Operator,
    TValue Value);