namespace Cnblogs.Architecture.Ddd.Cqrs.Dapper.Clickhouse;

/// <summary>
///     The collection for clickhouse contexts.
/// </summary>
public class ClickhouseContextCollection
{
    internal List<Type> ContextTypes { get; } = [];

    internal void Add<TContext>()
    {
        ContextTypes.Add(typeof(TContext));
    }
}
