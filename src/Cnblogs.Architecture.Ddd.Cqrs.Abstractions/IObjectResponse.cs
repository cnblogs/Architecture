namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     包含结果对象的响应。
/// </summary>
public interface IObjectResponse
{
    /// <summary>
    ///     获取结果。
    /// </summary>
    /// <returns>结果。</returns>
    public object? GetResult();
}