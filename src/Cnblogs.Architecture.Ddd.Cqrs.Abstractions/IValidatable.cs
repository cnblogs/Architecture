namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     定义可验证的类型。
/// </summary>
public interface IValidatable
{
    /// <summary>
    ///     验证方法，出错时返回 <see cref="ValidationError"/>，否则返回 <code>null</code>。
    /// </summary>
    ValidationError? Validate();
}