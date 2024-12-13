namespace SqlScribe.Builders;

public partial class SqlQueryBuilder
{
    public SqlQueryBuilder Delete()
    {
        _selectQueue.Clear();
        _runDelete = true;
        return this;
    }
}