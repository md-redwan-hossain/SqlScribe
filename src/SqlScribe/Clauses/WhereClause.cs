using System.Linq.Expressions;
using SqlScribe.Enums;

namespace SqlScribe.Clauses;

public record WhereClause<TEntity, TValue>(Expression<Func<TEntity, TValue>> Selector,
    SqlOperator Operator, TValue Value);
