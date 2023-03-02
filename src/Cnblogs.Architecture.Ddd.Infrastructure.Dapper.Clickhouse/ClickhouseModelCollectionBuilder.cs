namespace Cnblogs.Architecture.Ddd.Infrastructure.Dapper.Clickhouse;

/// <summary>
///     Configure models for clickhouse.
/// </summary>
public class ClickhouseModelCollectionBuilder
{
    private readonly Dictionary<Type, IClickhouseModelBuilder> _builders = new();

    /// <summary>
    ///     Register an entity to context.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    /// <returns><see cref="ClickhouseModelBuilder{T}"/>.</returns>
    public ClickhouseModelBuilder<T> Entity<T>()
        where T : class
    {
        var type = typeof(T);
        if (_builders.TryGetValue(type, out var builder))
        {
            return (ClickhouseModelBuilder<T>)builder;
        }

        var modelBuilder = new ClickhouseModelBuilder<T>();
        _builders.Add(typeof(T), modelBuilder);
        return modelBuilder;
    }

    internal void Build(ClickhouseContextOptions options)
    {
        foreach (var (key, value) in _builders)
        {
            options.Add(key, value.Build());
        }
    }
}
