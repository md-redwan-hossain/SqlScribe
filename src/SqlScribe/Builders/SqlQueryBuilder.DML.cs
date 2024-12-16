namespace SqlScribe.Builders;

public partial class SqlQueryBuilder
{
    public SqlQueryBuilder Delete()
    {
        _hasDeleteStatement = true;
        return this;
    }
}