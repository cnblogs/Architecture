using System.Linq.Expressions;

namespace Cnblogs.Architecture.TestShared;

public partial class EntityGenerator<TEntity>
{
    /// <summary>
    ///     Fill the property of all entities with boolean sequences like <c>[true, false, true, false, true...]</c>.
    /// </summary>
    /// <param name="propertyAccess">The property to be filled.</param>
    /// <returns>The generator with all entities been configured.</returns>
    public EntityGenerator<TEntity> FillWithBoolean(Expression<Func<TEntity, bool>> propertyAccess)
        => FillWith(propertyAccess, true, false);

    /// <summary>
    ///     Fill the property of all entities with int sequences like <c>[min, min+1, ..., max, min, min+1...]</c>
    /// </summary>
    /// <param name="propertyAccess">The property to be filled.</param>
    /// <param name="min">int sequence starts with.</param>
    /// <param name="max">int sequence ends with.</param>
    /// <returns>The generator with all entities been configured.</returns>
    public EntityGenerator<TEntity> FillWithInt(Expression<Func<TEntity, int>> propertyAccess, int min, int max)
        => FillWith(propertyAccess, Enumerable.Range(min, max - min));

    /// <summary>
    ///     Fill all entities with setup sequences. Loop <paramref name="setups"/> if there are more entities than setups.
    /// </summary>
    /// <param name="setups">The setups to be filled.</param>
    /// <returns>The generator with all entities been configured.</returns>
    public EntityGenerator<TEntity> FillWithSetup(params Action<TEntity>[] setups)
    {
        var cursor = 0;
        foreach (var entity in _template)
        {
            if (cursor == setups.Length)
            {
                cursor = 0;
            }

            setups[cursor](entity);
            cursor++;
        }

        return this;
    }

    /// <summary>
    ///     Fill the property of all entities with given values. Loop <paramref name="fillers"/> if there are more entities than fillers.
    /// </summary>
    /// <param name="propertyAccess">The property to be filled.</param>
    /// <param name="fillers">The value to fill.</param>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <returns>The generator with all entities been configured.</returns>
    public EntityGenerator<TEntity> FillWith<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyAccess,
        IEnumerable<TProperty> fillers)
        => FillWith(propertyAccess, fillers.ToArray());

    /// <summary>
    ///     Fill the property of all entities with given values. Loop <paramref name="fillers"/> if there are more entities than fillers.
    /// </summary>
    /// <param name="propertyAccess">The property to be filled.</param>
    /// <param name="fillers">The value to fill.</param>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <returns>The generator with all entities been configured.</returns>
    public EntityGenerator<TEntity> FillWith<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyAccess,
        params TProperty[] fillers)
    {
        var cursor = 0;
        var setter = GetPropertyInfo(propertyAccess);
        foreach (var entity in _template)
        {
            if (cursor == fillers.Length)
            {
                cursor = 0;
            }

            setter?.SetValue(entity, fillers[cursor]);
            cursor++;
        }

        return this;
    }
}