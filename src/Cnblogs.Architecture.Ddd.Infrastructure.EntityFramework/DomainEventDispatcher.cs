using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace Cnblogs.Architecture.Ddd.Infrastructure.EntityFramework;

/// <summary>
///     发布领域事件的扩展方法。
/// </summary>
public static class DomainEventDispatcher
{
    /// <summary>
    ///     提取 <paramref name="ctx" /> 中的领域事件，实体中的领域事件将被清除。
    /// </summary>
    /// <param name="ctx">当前 <see cref="DbContext" />。</param>
    /// <returns>提取的领域事件。</returns>
    public static List<IDomainEventSource> ExtractDomainEventSources(this DbContext ctx)
    {
        var domainEntities = ctx.ChangeTracker
            .Entries<IDomainEventSource>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        return domainEntities;
    }
}