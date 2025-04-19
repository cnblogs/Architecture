using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

using FluentAssertions;

using MediatR;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Behaviors;

public class CacheBehaviorTests
{
    [Fact]
    public async Task CacheBehavior_DisableCache_NotCacheAsync()
    {
        // Arrange
        var local = Substitute.For<ILocalCacheProvider>();
        local.AddCacheValue("cacheKey", "cacheValue");
        var remote = Substitute.For<IRemoteCacheProvider>();
        remote.AddCacheValue("cacheKey", "cacheValue");
        var behavior = GetBehavior<FakeQuery<string>, string>([local, remote]);

        // Act
        var result = await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.DisabledCache
            },
            _ => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        result.Should().Be("noCache");
    }

    [Fact]
    public async Task CacheBehavior_EnableLocal_NoCache_UpdateAsync()
    {
        // Arrange
        var local = Substitute.For<ILocalCacheProvider>();
        var behavior = GetBehavior<FakeQuery<string>, string>([local]);

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
        result.Should().Be("noCache");
        await local.Received(1).UpdateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task CacheBehavior_EnableLocal_HasCache_UseCacheAsync()
    {
        // Arrange
        var local = Substitute.For<ILocalCacheProvider>();
        local.AddCacheValue("cacheKey", "cacheValue");
        var remote = Substitute.For<IRemoteCacheProvider>();
        var behavior = GetBehavior<FakeQuery<string>, string>([local, remote]);

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
        result.Should().Be("cacheValue");
        await local.Received(0).UpdateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>());
        await remote.Received(0).GetAsync<string>(Arg.Any<string>());
    }

    [Fact]
    public async Task CacheBehavior_EnableRemote_NoCache_UpdateAsync()
    {
        // Arrange
        var remote = Substitute.For<IRemoteCacheProvider>();
        var local = Substitute.For<ILocalCacheProvider>();
        var behavior = GetBehavior<FakeQuery<string>, string>([local, remote]);

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
        result.Should().Be("noCache");
        await local.Received(0).UpdateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>());
        await remote.Received(1).UpdateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task CacheBehavior_EnableRemote_HasCache_UseCacheAsync()
    {
        // Arrange
        var remote = Substitute.For<IRemoteCacheProvider>().AddCacheValue("cacheKey", "cacheValue");
        var local = Substitute.For<ILocalCacheProvider>();
        var behavior = GetBehavior<FakeQuery<string>, string>([local, remote]);

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
        result.Should().Be("cacheValue");
        await local.Received(0).GetAsync<string>(Arg.Any<string>());
    }

    [Fact]
    public async Task CacheBehavior_ThrowOnGet_ThrowAsync()
    {
        // Arrange
        var remote = Substitute.For<IRemoteCacheProvider>();
        remote.GetAsync<string>(Arg.Any<string>()).ThrowsAsync(new Exception("test"));
        var behavior = GetBehavior<FakeQuery<string>, string>(
            [remote],
            o => o.ThrowIfFailedOnGet = true);

        // Act
        var act = async () => await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                RemoteExpires = TimeSpan.FromSeconds(1)
            },
            _ => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CacheBehavior_ThrowOnGet_NoThrowAsync()
    {
        // Arrange
        var remote = Substitute.For<IRemoteCacheProvider>();
        remote.GetAsync<string>(Arg.Any<string>()).ThrowsAsync(new Exception("test"));
        var behavior = GetBehavior<FakeQuery<string>, string>(
            [remote],
            o => o.ThrowIfFailedOnGet = false);

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
        result.Should().Be("noCache");
    }

    [Fact]
    public async Task CacheBehavior_ThrowOnUpdate_ThrowAsync()
    {
        // Arrange
        var remote = Substitute.For<IRemoteCacheProvider>();
        remote.UpdateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>())
            .ThrowsAsync(new Exception("test"));
        var behavior = GetBehavior<FakeQuery<string>, string>(
            [remote],
            o => o.ThrowIfFailedOnUpdate = true);

        // Act
        var act = async () => await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                RemoteExpires = TimeSpan.FromSeconds(1)
            },
            _ => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CacheBehavior_NotThrowOnUpdate_NotThrowAsync()
    {
        // Arrange
        var remote = Substitute.For<IRemoteCacheProvider>();
        remote.UpdateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>())
            .ThrowsAsync(new Exception("test"));
        var behavior = GetBehavior<FakeQuery<string>, string>(
            [remote],
            o => o.ThrowIfFailedOnUpdate = false);

        // Act
        var act = async () => await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                RemoteExpires = TimeSpan.FromSeconds(1)
            },
            _ => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<Exception>();
    }

    [Fact]
    public void CacheBehavior_NoProvider_Throw()
    {
        // Act
        var act = () => GetBehavior<FakeQuery<string>, string>([]);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    private static CacheableRequestBehavior<TRequest, TResponse> GetBehavior<TRequest, TResponse>(
        List<ICacheProvider> providers,
        Action<CacheableRequestOptions>? optionConfigure = null)
        where TRequest : ICachableRequest, IRequest<TResponse>
    {
        var option = new CacheableRequestOptions();
        optionConfigure?.Invoke(option);

        return new CacheableRequestBehavior<TRequest, TResponse>(
            providers,
            new DefaultDateTimeProvider(),
            new OptionsWrapper<CacheableRequestOptions>(option),
            NullLogger<CacheableRequestBehavior<TRequest, TResponse>>.Instance);
    }
}
