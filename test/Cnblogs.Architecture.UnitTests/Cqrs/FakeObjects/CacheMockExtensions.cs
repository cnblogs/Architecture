using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public static class CacheMockExtensions
{
    public static ICacheProvider AddCacheValue<T>(this ILocalCacheProvider mock, string key, T value)
    {
        mock.GetAsync<T>(key.ToLower())
            .Returns(new CacheEntry<T>(value, DateTimeOffset.Now.ToUnixTimeSeconds()));
        return mock;
    }

    public static IRemoteCacheProvider AddCacheValue<T>(this IRemoteCacheProvider mock, string key, T value)
    {
        mock.GetAsync<T>(key.ToLower())
            .Returns(new CacheEntry<T>(value, DateTimeOffset.Now.ToUnixTimeSeconds()));
        return mock;
    }
}
