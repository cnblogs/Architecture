using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Represents a <see cref="IOrderedQuery{TList}"/> with paged results.
/// </summary>
/// <typeparam name="TModel">The model to query.</typeparam>
public interface IPageableModelQuery<TModel> : IPageableQuery<TModel>
    where TModel : IModel;
