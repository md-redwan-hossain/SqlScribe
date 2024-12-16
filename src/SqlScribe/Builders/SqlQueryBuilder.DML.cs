namespace SqlScribe.Builders;

public partial class SqlQueryBuilder<TEntity>
{
    public SqlQueryBuilder<TEntity> Delete()
    {
        _hasDeleteStatement = true;
        return this;
    }
}