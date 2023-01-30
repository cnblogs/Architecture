namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
/// 实体接口。
/// </summary>
/// <typeparam name="TKey">键类型。</typeparam>
public interface IEntity<TKey>
{
    /// <summary>
    /// 实体的唯一 Id。
    /// </summary>
    TKey Id { get; set; }
}