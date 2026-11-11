using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cnblogs.Architecture.Ddd.Infrastructure.EntityFramework;

/// <summary>
///     实体配置使用的扩展方法。
/// </summary>
public static class EntityConfigurationExtensions
{
    /// <summary>
    ///     Ignore DateAdded, DateUpdated, Deleted properties that inherit from <see cref="Entity"/>.
    /// </summary>
    /// <param name="builder">
    ///     <see cref="EntityTypeBuilder{TEntity}" />
    /// </param>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>配置好的 <see cref="EntityTypeBuilder{TEntity}" /></returns>
    [Obsolete(
        "Prefer configure your entity to inherit directly from EntityBase and IEntity instead of using this method.")]
    public static EntityTypeBuilder<TEntity> IgnoreEntityDefaultProperties<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : Entity
    {
        return builder.Ignore(x => x.Deleted).Ignore(x => x.DateAdded).Ignore(x => x.DateUpdated);
    }

    /// <summary>
    ///     Configure default values for entity.
    /// </summary>
    /// <param name="builder"><see cref="EntityTypeBuilder{TEntity}"/></param>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <remarks>
    /// Equivalent to:
    /// builder.Property(x => x.DateAdded).HasDefaultValueSql("sysdatetimeoffset()");
    /// builder.Property(x => x.DateUpdated).HasDefaultValueSql("sysdatetimeoffset()");
    /// builder.Property(x => x.Deleted).HasDefaultValue(false);
    /// </remarks>
    /// <returns></returns>
    public static EntityTypeBuilder<TEntity> ConfigureEntityPropertyDefaults<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : Entity
    {
        builder.Property(x => x.DateAdded).HasDefaultValueSql("sysdatetimeoffset()");
        builder.Property(x => x.DateUpdated).HasDefaultValueSql("sysdatetimeoffset()");
        builder.Property(x => x.Deleted).HasDefaultValue(false);
        return builder;
    }

    /// <summary>
    ///    Do not generate foreign keys in migrations.
    /// </summary>
    /// <param name="modelBuilder"><see cref="ModelBuilder"/></param>
    /// <returns></returns>
    public static ModelBuilder ExcludeForeignKeysFromMigrations(this ModelBuilder modelBuilder)
    {
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.SetIsExcludedFromMigrations(true);
        }

        return modelBuilder;
    }
}
