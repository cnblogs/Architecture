using System.Data;
using Cnblogs.Architecture.Ddd.Infrastructure.Dapper;
using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Dapper;

public class TestDbConnectionFactory : IDbConnectionFactory
{
    /// <inheritdoc />
    public IDbConnection CreateDbConnection()
    {
        return Substitute.For<IDbConnection>();
    }
}
