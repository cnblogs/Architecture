using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using Moq;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public static class CacheMockExtensions
{
    public static Mock<ICacheProvider> AddCacheValue<T>(this Mock<ICacheProvider> mock, string key, T value)
    {
        mock.As<ILocalCacheProvider>()
            .Setup(x => x.GetAsync<T>(key.ToLower()))
            .ReturnsAsync(new CacheEntry<T>(value, DateTimeOffset.Now.ToUnixTimeSeconds()));
        mock.As<IRemoteCacheProvider>()
            .Setup(x => x.GetAsync<T>(key.ToLower()))
            .ReturnsAsync(new CacheEntry<T>(value, DateTimeOffset.Now.ToUnixTimeSeconds()));
        return mock;
    }

    public static Mock<IRemoteCacheProvider> AddCacheValue<T>(this Mock<IRemoteCacheProvider> mock, string key, T value)
    {
        mock.Setup(x => x.GetAsync<T>(key.ToLower()))
            .ReturnsAsync(new CacheEntry<T>(value, DateTimeOffset.Now.ToUnixTimeSeconds()));
        return mock;
    }
}