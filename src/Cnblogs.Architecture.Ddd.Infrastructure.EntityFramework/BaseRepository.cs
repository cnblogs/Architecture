using System.Linq.Expressions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cnblogs.Architecture.Ddd.Infrastructure.EntityFramework;

/// <summary>
///     仓储基类。
/// </summary>
/// <typeparam name="TContext">用于实际存储的 <see cref="DbContext" />。</typeparam>
/// <typeparam name="TEntity">该类管理的实体。</typeparam>
/// <typeparam name="TKey"><typeparamref name="TEntity" /> 的主键。</typeparam>
public abstract class BaseRepository<TContext, TEntity, TKey>
    : INavigationRepository<TEntity, TKey>, IUnitOfWork<TEntity, TKey>
    where TContext : DbContext
    where TEntity : EntityBase, IEntity<TKey>, IAggregateRoot
    where TKey : IComparable<TKey>
{
    private readonly IMediator _mediator;

    /// <summary>
    ///     初始化仓储类。
    /// </summary>
    /// <param name="mediator">领域事件总线。</param>
    /// <param name="context"><see cref="DbContext" />。</param>
    protected BaseRepository(IMediator mediator, TContext context)
    {
        Context = context;
        _mediator = mediator;
    }

    /// <summary>
    ///     底层 DbContext。
    /// </summary>
    protected TContext Context { get; }

    /// <inheritdoc />
    public IQueryable<TEntity> NoTrackingQueryable => GetNoTrackingQueryable<TEntity>();

    /// <inheritdoc />
    public IUnitOfWork<TEntity, TKey> UnitOfWork => this;

    /// <inheritdoc />
    public IQueryable<T> GetNoTrackingQueryable<T>()
        where T : class
    {
        return Context.Set<T>().AsNoTracking();
    }

    /// <inheritdoc />
    public async Task<TEntity> AddAsync(TEntity entity)
    {
        await Context.AddAsync(entity);
        await SaveEntitiesInternalAsync(true);
        return entity;
    }

    /// <inheritdoc />
    public async Task<TEnumerable> AddRangeAsync<TEnumerable>(TEnumerable entities)
        where TEnumerable : IEnumerable<TEntity>
    {
        await Context.AddRangeAsync(entities);
        await SaveEntitiesInternalAsync(true);
        return entities;
    }

    /// <inheritdoc cref="IRepository{TEntity,TKey}.GetAsync" />
    public async Task<TEntity?> GetAsync(TKey key)
    {
        return await Context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id.Equals(key));
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetAsync(TKey key, params Expression<Func<TEntity, object?>>[] includes)
    {
        return await Context.Set<TEntity>().AggregateIncludes(includes).FirstOrDefaultAsync(e => e.Id.Equals(key));
    }

    /// <inheritdoc />
    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        await SaveEntitiesInternalAsync(true);
        return entity;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        await SaveEntitiesInternalAsync(true);
        return entities;
    }

    /// <inheritdoc />
    public async Task<TEntity> DeleteAsync(TEntity entity)
    {
        Context.Remove(entity);
        await SaveEntitiesInternalAsync(true);
        return entity;
    }

    /// <inheritdoc />
    public TEntity Add(TEntity entity)
    {
        Context.Add(entity);
        return entity;
    }

    /// <inheritdoc />
    public TEntity Update(TEntity entity)
    {
        Context.Update(entity);
        return entity;
    }

    /// <inheritdoc />
    public TEntity Delete(TEntity entity)
    {
        Context.Remove(entity);
        return entity;
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        CallBeforeUpdate();
        return await Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        return SaveEntitiesInternalAsync(false, cancellationToken);
    }

    /// <summary>
    ///     发送领域事件之前执行，可用于持久化领域事件，记录日志等
    /// </summary>
    /// <param name="events">即将发送的领域事件。</param>
    /// <param name="context">数据库 <see cref="DbContext" />。</param>
    /// <returns></returns>
    protected virtual Task BeforeDispatchDomainEventAsync(List<DomainEvent> events, DbContext context)
    {
        return Task.CompletedTask;
    }

    private async Task<bool> SaveEntitiesInternalAsync(
        bool dispatchDomainEventFirst,
        CancellationToken cancellationToken = default)
    {
        var entities = Context.ExtractDomainEventSources();
        var domainEvents = entities.SelectMany(x => x.DomainEvents!.OfType<DomainEvent>()).ToList();
        entities.ForEach(x => x.ClearDomainEvents());
        if (dispatchDomainEventFirst)
        {
            await SaveChangesAsync(cancellationToken);
        }

        await BeforeDispatchDomainEventAsync(domainEvents, Context);
        await _mediator.DispatchDomainEventsAsync(domainEvents);
        if (dispatchDomainEventFirst == false)
        {
            await SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    private void CallBeforeUpdate()
    {
        var domainEntities = Context.ChangeTracker
            .Entries<EntityBase>()
            .Where(x => x.State != EntityState.Unchanged)
            .ToList();
        domainEntities.ForEach(x => x.Entity.BeforeUpdate());
    }
}