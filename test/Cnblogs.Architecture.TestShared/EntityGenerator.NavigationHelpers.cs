using System.Linq.Expressions;

namespace Cnblogs.Architecture.TestShared;

public partial class EntityGenerator<TEntity>
{
    public EntityGenerator<TEntity> HasOneForEachEntity<TNavigation>(
        Expression<Func<TEntity, TNavigation>> navigationTo,
        Func<TEntity, TNavigation> getNavigations)
    {
        return HasOneForEachEntity(navigationTo, null, getNavigations);
    }

    public EntityGenerator<TEntity> HasOneForEachEntity<TNavigation>(
        Expression<Func<TEntity, TNavigation>> navigationTo,
        TNavigation navigationEntity)
    {
        return HasOneForEachEntity(navigationTo, null, navigationEntity);
    }

    public EntityGenerator<TEntity> HasOneForEachEntity<TNavigation>(
        Expression<Func<TEntity, TNavigation>> navigationTo,
        Expression<Func<TNavigation, TEntity>>? navigationBack,
        TNavigation navigationEntity)
    {
        return HasOneForEachEntity(navigationTo, navigationBack, _ => navigationEntity);
    }

    public EntityGenerator<TEntity> HasOneForEachEntity<TNavigation>(
        Expression<Func<TEntity, TNavigation>> navigationTo,
        Expression<Func<TNavigation, TEntity>>? navigationBack,
        Func<TEntity, TNavigation> getNavigations)
    {
        var propertyTo = GetPropertyInfo(navigationTo);
        var propertyBack = GetPropertyInfo(navigationBack);

        if (propertyTo == null)
        {
            throw new ArgumentException("navigation should be settable property", nameof(navigationTo));
        }

        foreach (var template in _template)
        {
            var navigationEntity = getNavigations(template);
            var toSet = CloneEntity(navigationEntity);
            if (propertyBack != null)
            {
                propertyBack.SetValue(navigationEntity, template);
            }

            propertyTo.SetValue(template, toSet);
        }

        return this;
    }

    public EntityGenerator<TEntity> HasManyForEachEntity<TNavigation>(
        Expression<Func<TEntity, IEnumerable<TNavigation>>> navigationTo,
        List<TNavigation> navigationEntities)
    {
        return HasManyForEachEntity(navigationTo, null, navigationEntities);
    }

    public EntityGenerator<TEntity> HasManyForEachEntity<TNavigation>(
        Expression<Func<TEntity, IEnumerable<TNavigation>>> navigationTo,
        Func<TEntity, List<TNavigation>> getNavigations)
    {
        return HasManyForEachEntity(navigationTo, null, getNavigations);
    }

    public EntityGenerator<TEntity> HasManyForEachEntity<TNavigation>(
        Expression<Func<TEntity, IEnumerable<TNavigation>>> navigationTo,
        Expression<Func<TNavigation, TEntity>>? navigationBack,
        List<TNavigation> navigationEntities)
    {
        return HasManyForEachEntity(navigationTo, navigationBack, _ => navigationEntities);
    }

    public EntityGenerator<TEntity> HasManyForEachEntity<TNavigation>(
        Expression<Func<TEntity, IEnumerable<TNavigation>>> navigationTo,
        Expression<Func<TNavigation, TEntity>>? navigationBack,
        Func<TEntity, List<TNavigation>> getNavigations)
    {
        var propertyTo = GetPropertyInfo(navigationTo);
        var propertyBack = GetPropertyInfo(navigationBack);

        if (propertyTo == null)
        {
            throw new ArgumentException("navigation should be settable property", nameof(navigationTo));
        }

        foreach (var template in _template)
        {
            var navigationEntities = getNavigations(template);
            var toSet = CloneEntityList(navigationEntities);
            if (propertyBack != null)
            {
                toSet.ForEach(t => propertyBack.SetValue(t, template));
            }

            propertyTo.SetValue(template, toSet);
        }

        return this;
    }
}