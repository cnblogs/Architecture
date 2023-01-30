using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Cnblogs.Architecture.TestShared;

public partial class EntityGenerator<TEntity>
{
    private static List<T> CloneEntityList<T>(IEnumerable<T> templates)
        => templates.Select(CloneEntity).ToList();

    private static T CloneEntity<T>(T template)
    {
        var json = JsonSerializer.Serialize(template);
        return JsonSerializer.Deserialize<T>(json)!;
    }

    private static PropertyInfo? GetPropertyInfo<TFrom, TProperty>(
        Expression<Func<TFrom, TProperty>>? propertyExpression)
    {
        return (propertyExpression?.Body as MemberExpression)?.Member as PropertyInfo;
    }

    public static implicit operator TEntity(EntityGenerator<TEntity> entityGenerator)
        => entityGenerator.GenerateSingle();

    public static implicit operator List<TEntity>(EntityGenerator<TEntity> entityGenerator)
        => entityGenerator.Generate();
}