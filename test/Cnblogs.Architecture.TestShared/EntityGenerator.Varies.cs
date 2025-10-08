using System.Linq.Expressions;

namespace Cnblogs.Architecture.TestShared;

public partial class EntityGenerator<TEntity>
{
    public EntityGenerator<TEntity> VaryBy<TProperty>(
        Expression<Func<TEntity, TProperty>>? propertyAccess,
        IEnumerable<TProperty> variations)
    {
        var origin = CloneEntityList(_template);
        _template.Clear();

        var setter = GetPropertyInfo(propertyAccess);
        foreach (var variation in variations)
        {
            var list = CloneEntityList(origin);
            list.ForEach(t => setter?.SetValue(t, variation));
            _template.AddRange(list);
        }

        return this;
    }

    public EntityGenerator<TEntity> VaryBy<TProperty>(
        Expression<Func<TEntity, TProperty>>? propertyAccess,
        params TProperty[] variations)
        => VaryBy(propertyAccess, variations.AsEnumerable());

    public EntityGenerator<TEntity> VaryBySetup(Action<TEntity> setup)
    {
        var clone = CloneEntityList(_template);
        clone.ForEach(setup);
        _template.AddRange(clone);
        return this;
    }

    public EntityGenerator<TEntity> VaryByDateTimeDay(
        Expression<Func<TEntity, DateTime>>? datetimeAccess,
        int days)
        => VaryByDateTime(
            datetimeAccess,
            (start, day) => start.AddDays(-day),
            days,
            DateTime.Now);

    public EntityGenerator<TEntity> VaryByDateTimeDay(
        Expression<Func<TEntity, DateTimeOffset>>? datetimeAccess,
        int days)
        => VaryByDateTime(
            datetimeAccess,
            (start, day) => start.AddDays(-day),
            days,
            DateTime.Now);

    public EntityGenerator<TEntity> VaryByDateTime(
        Expression<Func<TEntity, DateTime>>? datetimeAccess,
        Func<DateTime, int, DateTime> timeDiffer,
        int diffs,
        DateTime startDate)
    {
        var dates = new DateTime[diffs];
        for (var i = 0; i < diffs; i++)
        {
            dates[i] = timeDiffer(startDate, i);
        }

        return VaryBy(datetimeAccess, dates);
    }

    public EntityGenerator<TEntity> VaryByDateTime(
        Expression<Func<TEntity, DateTimeOffset>>? datetimeAccess,
        Func<DateTimeOffset, int, DateTimeOffset> timeDiffer,
        int diffs,
        DateTime startDate)
    {
        var dates = new DateTimeOffset[diffs];
        for (var i = 0; i < diffs; i++)
        {
            dates[i] = timeDiffer(startDate, i);
        }

        return VaryBy(datetimeAccess, dates);
    }

    public EntityGenerator<TEntity> VaryByBoolean(Expression<Func<TEntity, bool>>? booleanAccess)
        => VaryBy(booleanAccess, true, false);

    public EntityGenerator<TEntity> VaryByInt(Expression<Func<TEntity, int>>? intAccess, int min, int max)
        => VaryBy(intAccess, Enumerable.Range(min, max - min));

    public EntityGenerator<TEntity> MultiplyBy(int times)
        => VaryBy<int>(null, Enumerable.Range(0, times));
}
