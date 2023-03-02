namespace Cnblogs.Architecture.Ddd.Infrastructure.Dapper.Clickhouse;

internal interface IClickhouseModelBuilder
{
    ClickhouseEntityConfiguration Build();
}
