using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
/// A generic model query handler.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TModel">The type of the model.</typeparam>
public interface IModelQueryHandler<TQuery, TModel> : IQueryHandler<TQuery, TModel>
    where TQuery : IModelQuery<TModel>
    where TModel : IModel;
