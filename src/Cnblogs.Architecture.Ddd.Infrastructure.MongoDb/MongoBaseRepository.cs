using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using MediatR;
using MongoDB.Driver;

namespace Cnblogs.Architecture.Ddd.Infrastructure.MongoDb;

/// <summary>
///     MongoDb Repository 基础实现类。
/// </summary>
/// <typeparam name="TContext">MongoContext 类型。</typeparam>
/// <typeparam name="TEntity">实体类型。</typeparam>
/// <typeparam name="TKey">主键类型。</typeparam>
public class MongoBaseRepository<TContext, TEntity, TKey> : IRepository<TEntity, TKey>, IUnitOfWork<TEntity, TKey>
    where TContext : MongoContext
    where TEntity : EntityBase, IEntity<TKey>, IAggregateRoot
    where TKey : IComparable<TKey>
{
    private readonly IMediator _mediator;
    private Dictionary<TKey, TEntity>? _toAdd;
    private Dictionary<TKey, TEntity>? _toUpdate;
    private Dictionary<TKey, TEntity>? _toDelete;

    /// <summary>
    ///     生成一个 <see cref="MongoBaseRepository{TContext,TEntity,TKey}"/>
    /// </summary>
    /// <param name="context">实体所在的 MongoContext。</param>
    /// <param name="mediator">事件发布总线。</param>
    public MongoBaseRepository(TContext context, IMediator mediator)
    {
        Context = context;
        _mediator = mediator;
    }

    /// <summary>
    ///     底层数据库上下文。
    /// </summary>
    public TContext Context { get; }

    /// <inheritdoc />
    public IQueryable<TEntity> NoTrackingQueryable => Context.Set<TEntity>().AsQueryable();

    /// <inheritdoc />
    public IUnitOfWork<TEntity, TKey> UnitOfWork => this;

    /// <inheritdoc />
    public async Task<TEntity> AddAsync(TEntity entity)
    {
        var collection = Context.Set<TEntity>();
        EnsureBeforeUpdateCalled(entity);
        var domainEvents = ExtractDomainEvents(entity);
        await collection.InsertOneAsync(entity);
        await DispatchDomainEventsAsync(domainEvents);
        return entity;
    }

    /// <inheritdoc />
    public async Task<TEnumerable> AddRangeAsync<TEnumerable>(TEnumerable entities)
        where TEnumerable : IEnumerable<TEntity>
    {
        var collection = Context.Set<TEntity>();
        var toAdd = entities.ToList();
        toAdd.ForEach(EnsureBeforeUpdateCalled);
        var domainEvents = toAdd.Where(x => x.DomainEvents != null).SelectMany(x => ExtractDomainEvents(x)!).ToList();
        await collection.InsertManyAsync(toAdd);
        await DispatchDomainEventsAsync(domainEvents);
        return entities;
    }

    /// <inheritdoc />
    public async Task<TEntity> DeleteAsync(TEntity entity)
    {
        var collection = Context.Set<TEntity>();
        EnsureBeforeUpdateCalled(entity);
        var domainEvents = ExtractDomainEvents(entity);
        await collection.DeleteOneAsync(Builders<TEntity>.Filter.Eq(x => x.Id, entity.Id));
        await DispatchDomainEventsAsync(domainEvents);
        return entity;
    }

    /// <inheritdoc cref="IRepository{TEntity,TKey}.GetAsync" />
    public async Task<TEntity?> GetAsync(TKey key)
    {
        return await Context.Set<TEntity>().Find(Builders<TEntity>.Filter.Eq(x => x.Id, key)).FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public IQueryable<T> GetNoTrackingQueryable<T>()
        where T : class
    {
        return Context.Set<T>().AsQueryable();
    }

    /// <inheritdoc />
    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        var collection = Context.Set<TEntity>();
        EnsureBeforeUpdateCalled(entity);
        var domainEvents = ExtractDomainEvents(entity);
        await collection.ReplaceOneAsync(Builders<TEntity>.Filter.Eq(x => x.Id, entity.Id), entity);
        await DispatchDomainEventsAsync(domainEvents);
        return entity;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        var collection = Context.Set<TEntity>();
        var list = entities.ToList();
        list.ForEach(EnsureBeforeUpdateCalled);
        var domainEvents = list.Where(x => x.DomainEvents != null).SelectMany(x => ExtractDomainEvents(x)!).ToList();
        var updates = list.Select(x => new ReplaceOneModel<TEntity>(Builders<TEntity>.Filter.Eq(y => y.Id, x.Id), x));
        await collection.BulkWriteAsync(updates);
        await DispatchDomainEventsAsync(domainEvents);
        return list;
    }

    /// <summary>
    ///     发送领域事件之前执行，可用于持久化领域事件，记录日志等
    /// </summary>
    /// <param name="domainEvents">即将发送的领域事件。</param>
    /// <returns></returns>
    protected virtual Task BeforeDispatchDomainEventsAsync(IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        return Task.CompletedTask;
    }

    private static IReadOnlyCollection<IDomainEvent>? ExtractDomainEvents(TEntity entity)
    {
        var domainEvents = entity.DomainEvents?.OfType<DomainEvent>().ToList();
        if (domainEvents is null || domainEvents.Count == 0)
        {
            return null;
        }

        entity.ClearDomainEvents();
        return domainEvents;
    }

    private async Task DispatchDomainEventsAsync(IReadOnlyCollection<IDomainEvent>? domainEvents)
    {
        if (domainEvents is null || domainEvents.Any() == false)
        {
            return;
        }

        await BeforeDispatchDomainEventsAsync(domainEvents);
        await _mediator.DispatchDomainEventsAsync(domainEvents);
    }

    private static void EnsureBeforeUpdateCalled(TEntity entity) => entity.BeforeUpdate();

    /// <inheritdoc />
    public TEntity Add(TEntity entity)
    {
        if (_toDelete != null && _toDelete.ContainsKey(entity.Id))
        {
            // delete then insert == replace
            _toDelete.Remove(entity.Id);
            Update(entity);
            return entity;
        }

        if (_toUpdate != null && _toUpdate.ContainsKey(entity.Id))
        {
            // trying to add item with same key that been updated
            throw new InvalidOperationException($"Entity with same key has already been updated, key: {entity.Id}");
        }

        _toAdd ??= new Dictionary<TKey, TEntity>();
        if (_toAdd.ContainsKey(entity.Id))
        {
            throw new InvalidOperationException($"Entity with same key has already been added, key: {entity.Id}");
        }

        _toAdd.Add(entity.Id, entity);
        return entity;
    }

    /// <inheritdoc />
    public TEntity Update(TEntity entity)
    {
        if (_toDelete != null && _toDelete.ContainsKey(entity.Id))
        {
            // you can't delete item first then update it
            throw new InvalidOperationException($"Entity with same key has already been deleted, key: {entity.Id}");
        }

        _toUpdate ??= new Dictionary<TKey, TEntity>();
        _toUpdate[entity.Id] = entity;

        return entity;
    }

    /// <inheritdoc />
    public TEntity Delete(TEntity entity)
    {
        if (_toAdd != null && _toAdd.ContainsKey(entity.Id))
        {
            // entity has not been inserted yet, just preventing it from been inserted.
            _toAdd.Remove(entity.Id);
            return entity;
        }

        _toDelete ??= new Dictionary<TKey, TEntity>();
        _toDelete[entity.Id] = entity;

        return entity;
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.BulkWriteWithTransactionAsync(_toAdd, _toUpdate, _toDelete, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = new List<IDomainEvent>();
        if (_toAdd is { Count: > 0 })
        {
            Extract(_toAdd);
        }

        if (_toUpdate is { Count: > 0 })
        {
            Extract(_toUpdate);
        }

        if (_toDelete is { Count: > 0 })
        {
            Extract(_toDelete);
        }

        await DispatchDomainEventsAsync(domainEvents);
        await SaveChangesAsync(cancellationToken);
        return true;

        void Extract(Dictionary<TKey, TEntity> entities)
        {
            foreach (var entity in entities.Values)
            {
                EnsureBeforeUpdateCalled(entity);
                if (entity.DomainEvents != null)
                {
                    domainEvents.AddRange(ExtractDomainEvents(entity)!);
                }
            }
        }
    }
}