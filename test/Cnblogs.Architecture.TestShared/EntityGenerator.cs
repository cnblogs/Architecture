using System.Linq.Expressions;

namespace Cnblogs.Architecture.TestShared;

/// <summary>
///     Generator for testing entities.
/// </summary>
/// <typeparam name="TEntity">Entity type to generate.</typeparam>
public partial class EntityGenerator<TEntity>
{
    private readonly List<TEntity> _template = new();
    private readonly List<Action<TEntity>> _customCloneSetups = new();

    /// <summary>
    ///     Create entity based on given template.
    /// </summary>
    /// <param name="template">The entity template.</param>
    public EntityGenerator(TEntity template)
    {
        _template.Add(template);
    }

    /// <summary>
    ///     Add setup step for each entity template.
    /// </summary>
    /// <param name="setup">The setup step for each entity template.</param>
    /// <returns>The generator after executing setup for each entity template.</returns>
    public EntityGenerator<TEntity> Setup(Action<TEntity> setup)
    {
        _template.ForEach(setup);
        return this;
    }

    /// <summary>
    ///     Add setup step after each entity been cloned.
    /// </summary>
    /// <param name="setup">The setup step for cloned entity.</param>
    /// <returns>The generator after adding the custom clone setup.</returns>
    public EntityGenerator<TEntity> WithEntityCloneSetup(Action<TEntity> setup)
    {
        _customCloneSetups.Add(setup);
        return this;
    }

    /// <summary>
    ///     Add extra setup setup for specific property.
    /// </summary>
    /// <param name="propertyAccess">The property to configure.</param>
    /// <param name="generateFunc">The generate function for the property.</param>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <returns>The generator after adding the custom clone setup.</returns>
    public EntityGenerator<TEntity> WithPropertyCloneSetup<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyAccess,
        Func<TEntity, TProperty> generateFunc)
    {
        var property = GetPropertyInfo(propertyAccess);
        return WithEntityCloneSetup(e => property?.SetValue(e, generateFunc.Invoke(e)));
    }

    /// <summary>
    ///     Generate entities.
    /// </summary>
    /// <returns>The entities been generated.</returns>
    public List<TEntity> Generate()
    {
        return _template;
    }

    /// <summary>
    ///     Generate entity, only the first item would be returned if there were many.
    /// </summary>
    /// <returns>The entity been generate.</returns>
    public TEntity GenerateSingle()
    {
        return Generate().First();
    }

    private List<TEntity> CloneEntityList(IEnumerable<TEntity> templates) => templates.Select(CloneEntity).ToList();

    private TEntity CloneEntity(TEntity template)
    {
        var clone = EntityGenerator<TEntity>.CloneEntity(template);
        if (_customCloneSetups.Count > 0)
        {
            _customCloneSetups.ForEach(setup => setup(clone));
        }

        return clone;
    }
}