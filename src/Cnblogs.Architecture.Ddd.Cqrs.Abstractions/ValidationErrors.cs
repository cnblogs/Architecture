using System.Collections;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Collection of <see cref="ValidationError"/>.
/// </summary>
public class ValidationErrors : ICollection<ValidationError>
{
    private readonly List<ValidationError> _validationErrors = new();

    /// <summary>
    ///    Add a new validation error.
    /// </summary>
    /// <param name="validationError">The validation error.</param>
    public void Add(ValidationError validationError)
    {
        _validationErrors.Add(validationError);
    }

    /// <summary>
    ///     Clear all validation errors.
    /// </summary>
    public void Clear()
    {
        _validationErrors.Clear();
    }

    /// <inheritdoc />
    public bool Contains(ValidationError item) => _validationErrors.Contains(item);

    /// <inheritdoc />
    public void CopyTo(ValidationError[] array, int arrayIndex) => _validationErrors.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(ValidationError item) => _validationErrors.Remove(item);

    /// <inheritdoc />
    public int Count => _validationErrors.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public IEnumerator<ValidationError> GetEnumerator() => _validationErrors.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
