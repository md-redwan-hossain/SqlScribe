using System.Data.Common;

namespace SqlScribe.ExampleScaffolder.Persistence;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateConnectionAsync(CancellationToken ct = default);
}