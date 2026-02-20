using Cnblogs.Architecture.Ddd.Domain.Abstractions;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
/// A model query that suitable for generic handlers
/// </summary>
/// <typeparam name="TModel">The type of the model</typeparam>
public interface IModelQuery<TModel> : IQuery<TModel>
    where TModel : IModel;
