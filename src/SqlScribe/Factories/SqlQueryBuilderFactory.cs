using SqlScribe.Builders;
using SqlScribe.Enums;

namespace SqlScribe.Factories;

public class SqlQueryBuilderFactory
{
    private readonly DatabaseVendor _databaseVendor;
    private readonly SqlNamingConvention _namingConvention;
    private readonly bool _pluralizeTableName;

    public SqlQueryBuilderFactory(DatabaseVendor databaseVendor, SqlNamingConvention namingConvention,
        bool pluralizeTableName)
    {
        _namingConvention = namingConvention;
        _pluralizeTableName = pluralizeTableName;
        _databaseVendor = databaseVendor;
    }
    
    public SqlQueryBuilder CreateSqlQueryBuilder()
    {
        return new SqlQueryBuilder(_databaseVendor, _namingConvention, _pluralizeTableName);
    }
}