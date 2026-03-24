using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;
using MediatR;
using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Behaviors;

public class CacheBehaviorTests
{
    [Fact]
    public async Task CacheBehavior_DisableCache_NotCacheAsync()
    {
        // Arrange
        var local = Substitute.For<ICacheProvider>();
        var behavior = GetBehavior<FakeQuery<string>, string>(local);

        // Act
        var result = await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache, RemoteCacheBehavior = CacheBehavior.DisabledCache
            },
            _ => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        Assert.Equal("noCache", result);
    }

    [Fact]
    public async Task CacheBehavior_EnableLocal_NoCache_UpdateAsync()
    {
        // Arrange
        var local = Substitute.For<ICacheProvider>().MockCacheValue("cacheKey", "noCache");
        var behavior = GetBehavior<FakeQuery<string>, string>(local);

        // Act
        var result = await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                LocalExpires = TimeSpan.FromSeconds(1),
                RemoteCacheBehavior = CacheBehavior.DisabledCache
            },
            _ => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        Assert.Equal("noCache", result);
        await local.Received(1).GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<string>>>(),
            null,
            TimeSpan.FromSeconds(1),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CacheBehavior_EnableLocal_HasCache_UseCacheAsync()
    {
        // Arrange
        var cache = Substitute.For<ICacheProvider>().MockCacheValue("cacheKey", "cacheValue");
        var behavior = GetBehavior<FakeQuery<string>, string>(cache);

        // Act
        var result = await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                LocalExpires = TimeSpan.FromSeconds(1),
                RemoteCacheBehavior = CacheBehavior.DisabledCache
            },
            _ => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        Assert.Equal("cacheValue", result);
        await cache.Received(1).GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<string>>>(),
            null,
            TimeSpan.FromSeconds(1),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CacheBehavior_EnableRemote_NoCache_UpdateAsync()
    {
        // Arrange
        var cache = Substitute.For<ICacheProvider>().MockCacheValue("cacheKey", "noCache");
        var behavior = GetBehavior<FakeQuery<string>, string>(cache);

        // Act
        var result = await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                RemoteExpires = TimeSpan.FromSeconds(1)
            },
            _ => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        Assert.Equal("noCache", result);
        await cache.Received(1).GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<string>>>(),
            TimeSpan.FromSeconds(1),
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CacheBehavior_EnableRemote_HasCache_UseCacheAsync()
    {
        // Arrange
        var cache = Substitute.For<ICacheProvider>().MockCacheValue("cacheKey", "cacheValue");
        var behavior = GetBehavior<FakeQuery<string>, string>(cache);

        // Act
        var result = await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                RemoteExpires = TimeSpan.FromSeconds(1)
            },
            _ => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        Assert.Equal("cacheValue", result);
        await cache.Received(1).GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<string>>>(),
            TimeSpan.FromSeconds(1),
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    private static CacheableRequestBehavior<TRequest, TResponse> GetBehavior<TRequest, TResponse>(
        ICacheProvider provider)
        where TRequest : ICachableRequest, IRequest<TResponse>
    {
        return new CacheableRequestBehavior<TRequest, TResponse>(provider);
    }
}
