using Cnblogs.Architecture.Ddd.Domain.Abstractions;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cnblogs.Architecture.Ddd.Infrastructure.EntityFramework;

/// <summary>
///     实体配置使用的扩展方法。
/// </summary>
public static class EntityConfigurationExtensions
{
    /// <summary>
    ///     不使用实体自带的 DateAdded, DateUpdated, Deleted 属性。
    /// </summary>
    /// <param name="builder">
    ///     <see cref="EntityTypeBuilder{TEntity}" />
    /// </param>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    /// <returns>配置好的 <see cref="EntityTypeBuilder{TEntity}" /></returns>
    public static EntityTypeBuilder<TEntity> IgnoreEntityDefaultProperties<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : Entity
    {
        return builder.Ignore(x => x.Deleted).Ignore(x => x.DateAdded).Ignore(x => x.DateUpdated);
    }
}