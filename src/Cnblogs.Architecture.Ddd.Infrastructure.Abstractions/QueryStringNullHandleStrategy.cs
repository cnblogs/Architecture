namespace Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

/// <summary>
/// <see cref="QueryStringBuilder"/> 中的空置处理方式。
/// </summary>
public enum QueryStringNullHandleStrategy
{
    /// <summary>
    /// <c>null</c> 值将不会在查询字符串中出现，默认行为。
    /// </summary>
    Absent = 0,

    /// <summary>
    /// <c>null</c> 值将只保留键，例如 <c>?key1=&amp;key2=value</c>
    /// </summary>
    Empty = 1,
}