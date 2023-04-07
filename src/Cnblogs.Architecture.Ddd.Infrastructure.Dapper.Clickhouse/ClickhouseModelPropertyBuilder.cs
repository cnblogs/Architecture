using System.Reflection;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Dapper.Clickhouse;

/// <summary>
///     Configuration builder for clickhouse model property;
/// </summary>
/// <typeparam name="TEntity">The entity type that property belongs.</typeparam>
public class ClickhouseModelPropertyBuilder<TEntity>
{
    /// <summary>
    ///     Create a ClickhouseModelPropertyBuilder from entity builder.
    /// </summary>
    /// <param name="propertyInfo">The property been configured.</param>
    public ClickhouseModelPropertyBuilder(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
        ColumnName = propertyInfo.Name;
    }

    /// <summary>
    ///     Configure column name for this property.
    /// </summary>
    /// <param name="name">New column name.</param>
    /// <returns><see cref="ClickhouseModelPropertyBuilder{TEntity}"/></returns>
    public ClickhouseModelPropertyBuilder<TEntity> HasColumnName(string name)
    {
        ColumnName = name;
        return this;
    }

    /// <summary>
    ///     Ignore this property from mapping.
    /// </summary>
    /// <returns><see cref="ClickhouseModelPropertyBuilder{TEntity}"/></returns>
    public ClickhouseModelPropertyBuilder<TEntity> Ignore()
    {
        IsIgnored = true;
        return this;
    }

    internal string ColumnName { get; private set; }

    internal PropertyInfo PropertyInfo { get; }

    internal bool IsIgnored { get; private set; }
}
