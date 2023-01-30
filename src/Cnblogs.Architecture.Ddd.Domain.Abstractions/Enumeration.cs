using System.Reflection;

namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
///     枚举基类。
/// </summary>
public abstract class Enumeration : IComparable
{
    /// <summary>
    ///     构造一个枚举值。
    /// </summary>
    /// <param name="id">枚举 Id。</param>
    /// <param name="name">枚举值名称。</param>
    protected Enumeration(int id, string name)
    {
        (Id, Name) = (id, name);
    }

    /// <summary>
    ///     枚举值的名称。
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     枚举值的 Id。
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     比较两个枚举值。
    /// </summary>
    /// <param name="other">另一个枚举值。</param>
    /// <returns>比较结果。</returns>
    public int CompareTo(object? other)
    {
        return other is null ? 1 : Id.CompareTo(((Enumeration)other).Id);
    }

    /// <summary>
    ///     两个枚举值之间的绝对距离。
    /// </summary>
    /// <param name="firstValue">第一个值。</param>
    /// <param name="secondValue">第二个值。</param>
    /// <returns><paramref name="firstValue" /> 和 <paramref name="secondValue" /> 之间距离的绝对值。</returns>
    public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
    {
        var absoluteDifference = Math.Abs(firstValue.Id - secondValue.Id);
        return absoluteDifference;
    }

    /// <summary>
    ///     两个枚举是否相等。
    /// </summary>
    /// <param name="obj">另一个枚举。</param>
    /// <returns>是否相等。</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    /// <summary>
    ///     从枚举值名称获取对应的枚举对象。
    /// </summary>
    /// <param name="displayName">枚举名称。</param>
    /// <typeparam name="T">要获取的枚举类型。</typeparam>
    /// <returns>枚举对象。</returns>
    /// <exception cref="InvalidOperationException"><typeparamref name="T" /> 没有与 <paramref name="displayName" /> 对应的枚举值对象。</exception>
    public static T FromDisplayName<T>(string displayName)
        where T : Enumeration
    {
        var matchingItem = Parse<T, string>(displayName, "display name", item => item.Name == displayName);
        return matchingItem;
    }

    /// <summary>
    ///     从枚举值 Id 获取对应的枚举对象。
    /// </summary>
    /// <param name="value">枚举值 Id。</param>
    /// <typeparam name="T">要获取的枚举类型。</typeparam>
    /// <returns><paramref name="value" /> 对应的枚举值对象。</returns>
    /// <exception cref="InvalidOperationException"><typeparamref name="T" /> 没有与 <paramref name="value" /> 对应的枚举值对象。</exception>
    public static T FromValue<T>(int value)
        where T : Enumeration
    {
        var matchingItem = Parse<T, int>(value, "value", item => item.Id == value);
        return matchingItem;
    }

    /// <summary>
    ///     获取枚举所有可能的值。
    /// </summary>
    /// <typeparam name="T">枚举类型。</typeparam>
    /// <returns>所有可能的枚举值。</returns>
    public static IEnumerable<T> GetAll<T>()
        where T : Enumeration
    {
        return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();
    }

    /// <summary>
    ///     获取哈希值。
    /// </summary>
    /// <returns>哈希值。</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    private static T Parse<T, TFrom>(TFrom value, string description, Func<T, bool> predicate)
        where T : Enumeration
    {
        var matchingItem = GetAll<T>().FirstOrDefault(predicate);

        if (matchingItem == null)
        {
            throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");
        }

        return matchingItem;
    }

    /// <summary>
    ///     获取枚举值的 <see cref="Name" />。
    /// </summary>
    /// <returns><see cref="Name" />。</returns>
    public override string ToString()
    {
        return Name;
    }
}