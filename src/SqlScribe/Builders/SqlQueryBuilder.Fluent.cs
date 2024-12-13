using SqlScribe.Enums;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder
{
    public SqlQueryBuilder AddParenthesisInConditionalClause(SqlParenthesisType parenthesisType)
    {
        switch (parenthesisType)
        {
            case SqlParenthesisType.Opening:
                _conditionQueue.Enqueue(" ( ");
                break;
            case SqlParenthesisType.Closing:
                _conditionQueue.Enqueue(" ) ");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(parenthesisType), parenthesisType, null);
        }

        return this;
    }

    public SqlQueryBuilder And()
    {
        _conditionQueue.Enqueue(" AND ");
        return this;
    }

    public SqlQueryBuilder Or()
    {
        _conditionQueue.Enqueue(" OR ");
        return this;
    }
}