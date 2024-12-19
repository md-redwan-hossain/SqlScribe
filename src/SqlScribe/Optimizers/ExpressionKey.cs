using System;
using System.Linq;
using System.Linq.Expressions;

namespace SqlScribe.Optimizers;

internal readonly struct ExpressionKey : IEquatable<ExpressionKey>
{
    private readonly int _hashCode;
    private readonly Type? _bodyType;
    private readonly Type? _delegateType;
    private readonly string? _memberPath;

    public ExpressionKey(LambdaExpression expression)
    {
        _bodyType = expression.Body.Type;
        _delegateType = expression.Type;
        _memberPath = GetMemberPath(expression.Body);
        _hashCode = unchecked(17 * 23 + (_bodyType?.GetHashCode() ?? 0) * 23
                                      + (_delegateType?.GetHashCode() ?? 0) * 23
                                      + (_memberPath?.GetHashCode() ?? 0));
    }

    private static string GetMemberPath(Expression expression)
    {
        return expression switch
        {
            MemberExpression m => m.Member.Name,
            UnaryExpression { Operand: MemberExpression um } => um.Member.Name,
            NewExpression { Members: not null } n => string.Join("|", n.Members.Select(m => m.Name)),
            _ => string.Empty
        };
    }

    public override int GetHashCode() => _hashCode;

    public bool Equals(ExpressionKey other)
    {
        return _bodyType == other._bodyType &&
               _delegateType == other._delegateType &&
               _memberPath == other._memberPath;
    }

    public override bool Equals(object? obj) =>
        obj is ExpressionKey other && Equals(other);
}