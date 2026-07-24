namespace Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;

/// <summary>
///     Categorizes a CQRS request type for tool generation.
/// </summary>
internal enum RequestKind
{
    /// <summary>
    ///     The type is neither a Command nor a Query.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The type is a Command (<c>ICommand&lt;TError&gt;</c> or <c>ICommand&lt;TView, TError&gt;</c>).
    /// </summary>
    Command = 1,

    /// <summary>
    ///     The type is a Query (<c>IQuery&lt;&gt;</c>, <c>IListQuery&lt;&gt;</c>, <c>IOrderedQuery&lt;&gt;</c>, <c>IPageableQuery&lt;&gt;</c>, <c>IModelQuery&lt;&gt;</c>, or <c>IPageableModelQuery&lt;&gt;</c>).
    /// </summary>
    Query = 2,
}
