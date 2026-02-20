using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
/// Defines a pageable model query handler.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TModel">The type of the model</typeparam>
public interface IPageableModelQueryHandler<TQuery, TModel> : IPageableQueryHandler<TQuery, TModel>
    where TQuery : IPageableModelQuery<TModel>
    where TModel : IModel;
