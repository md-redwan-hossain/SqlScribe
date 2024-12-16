using SqlScribe.Enums;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder<TEntity>
{
    public SqlQueryBuilder<TEntity> AddParenthesis(SqlParenthesisType parenthesisType, SqlKeywordLocation location)
    {
        switch (parenthesisType)
        {
            case SqlParenthesisType.Opening:

                switch (location)
                {
                    case SqlKeywordLocation.WhereClause:
                        _whereQueue.Enqueue(" ( ");
                        break;
                    case SqlKeywordLocation.HavingClause:
                        _havingQueue.Enqueue(" ( ");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(location), location, null);
                }

                break;
            case SqlParenthesisType.Closing:

                switch (location)
                {
                    case SqlKeywordLocation.WhereClause:
                        _whereQueue.Enqueue(" ) ");
                        break;
                    case SqlKeywordLocation.HavingClause:
                        _havingQueue.Enqueue(" ) ");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(location), location, null);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(parenthesisType), parenthesisType, null);
        }

        return this;
    }

    public SqlQueryBuilder<TEntity> And(SqlKeywordLocation location)
    {
        switch (location)
        {
            case SqlKeywordLocation.WhereClause:
                _whereQueue.Enqueue(" AND ");
                break;
            case SqlKeywordLocation.HavingClause:
                _havingQueue.Enqueue(" AND ");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(location), location, null);
        }

        return this;
    }

    public SqlQueryBuilder<TEntity> Or(SqlKeywordLocation location)
    {
        switch (location)
        {
            case SqlKeywordLocation.WhereClause:
                _whereQueue.Enqueue(" OR ");
                break;
            case SqlKeywordLocation.HavingClause:
                _havingQueue.Enqueue(" OR ");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(location), location, null);
        }

        return this;
    }
}