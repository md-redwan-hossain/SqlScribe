using System.Data.Common;

namespace SqlScribe.HttpApiExample.Data;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateConnectionAsync(CancellationToken ct = default);
}