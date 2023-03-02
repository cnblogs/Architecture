using System.Reflection;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Dapper.Clickhouse;

internal record ClickhouseEntityConfiguration(string TableName, PropertyInfo[] Properties, string[] ColumnNames)
{
    internal object?[] ToObjectArray(object entity)
    {
        return Properties.Select(x => x.GetValue(entity)).ToArray();
    }
}
