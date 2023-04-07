using System.Linq.Expressions;
using System.Reflection;

namespace Cnblogs.Architecture.Ddd.Infrastructure.Dapper.Clickhouse;

/// <summary>
///     Configure mapping between clickhouse and C# class.
/// </summary>
/// <typeparam name="T">The type of model been configured.</typeparam>
public class ClickhouseModelBuilder<T> : IClickhouseModelBuilder
{
    private readonly Dictionary<string, ClickhouseModelPropertyBuilder<T>> _propertyBuilders;
    private string _tableName;

    internal ClickhouseModelBuilder()
    {
        _tableName = typeof(T).Name;
        _propertyBuilders = typeof(T).GetProperties().Where(x => x.GetGetMethod() != null)
            .Select(x => new ClickhouseModelPropertyBuilder<T>(x)).ToDictionary(x => x.PropertyInfo.Name, x => x);
    }

    /// <summary>
    ///     Map model type to specific table.
    /// </summary>
    /// <param name="tableName">The full name of table, includes database name. e.x. &lt;database&gt;.&lt;table&gt;</param>
    /// <returns><see cref="ClickhouseModelBuilder{T}"/>.</returns>
    public ClickhouseModelBuilder<T> ToTable(string tableName)
    {
        _tableName = tableName;
        return this;
    }

    /// <summary>
    ///     Start configure property.
    /// </summary>
    /// <param name="propertyGetter">The property been configured.</param>
    /// <typeparam name="TProperty">The type of property.</typeparam>
    /// <returns><see cref="ClickhouseModelPropertyBuilder{TEntity}"/>.</returns>
    /// <exception cref="InvalidOperationException">When no suitable property was fount by <paramref name="propertyGetter"/>.</exception>
    public ClickhouseModelPropertyBuilder<T> Property<TProperty>(Expression<Func<T, TProperty>> propertyGetter)
    {
        if ((propertyGetter.Body as MemberExpression)?.Member is not PropertyInfo propertyInfo)
        {
            throw new InvalidOperationException("No suitable property can be found from given expression");
        }

        var builder = _propertyBuilders.GetValueOrDefault(propertyInfo.Name);
        if (builder is null)
        {
            throw new InvalidOperationException($"No suitable property was found by name: {propertyInfo.Name}");
        }

        return builder;
    }

    ClickhouseEntityConfiguration IClickhouseModelBuilder.Build()
    {
        var builders = _propertyBuilders.Values.Where(x => x.IsIgnored == false).ToArray();
        return new ClickhouseEntityConfiguration(
            _tableName,
            builders.Select(x => x.PropertyInfo).ToArray(),
            builders.Select(x => x.ColumnName).ToArray());
    }
}
