using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Dapper;

public class TestDapperContext : DapperContext
{
    /// <inheritdoc />
    public TestDapperContext(
        IOptions<DbConnectionFactoryCollection> dbConnectionFactoryCollection,
        IServiceProvider sp)
        : base(dbConnectionFactoryCollection, sp)
    {
    }
}
