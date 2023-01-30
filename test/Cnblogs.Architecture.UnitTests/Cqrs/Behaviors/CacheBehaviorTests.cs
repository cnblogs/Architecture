using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

using FluentAssertions;

using MediatR;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Behaviors;

public class CacheBehaviorTests
{
    [Fact]
    public async Task CacheBehavior_DisableCache_NotCacheAsync()
    {
        // Arrange
        var local = new Mock<ICacheProvider>();
        local.AddCacheValue("cacheKey", "cacheValue");
        var behavior = GetBehavior<FakeQuery<string>, string>(new List<ICacheProvider> { local.Object });

        // Act
        var result = await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.DisabledCache
            },
            () => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        result.Should().Be("noCache");
    }

    [Fact]
    public async Task CacheBehavior_EnableLocal_NoCache_UpdateAsync()
    {
        // Arrange
        var local = new Mock<ILocalCacheProvider>();
        var behavior = GetBehavior<FakeQuery<string>, string>(new List<ICacheProvider> { local.Object });

        // Act
        var result = await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                LocalExpires = TimeSpan.FromSeconds(1),
                RemoteCacheBehavior = CacheBehavior.DisabledCache
            },
            () => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        result.Should().Be("noCache");
        local.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task CacheBehavior_EnableLocal_HasCache_UseCacheAsync()
    {
        // Arrange
        var local = new Mock<ICacheProvider>();
        local.AddCacheValue("cacheKey", "cacheValue");
        var remote = new Mock<IRemoteCacheProvider>();
        var behavior = GetBehavior<FakeQuery<string>, string>(new List<ICacheProvider> { local.Object, remote.Object });

        // Act
        var result = await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                LocalExpires = TimeSpan.FromSeconds(1),
                RemoteCacheBehavior = CacheBehavior.DisabledCache
            },
            () => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        result.Should().Be("cacheValue");
        local.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
        remote.Verify(x => x.GetAsync<string>(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CacheBehavior_EnableRemote_NoCache_UpdateAsync()
    {
        // Arrange
        var remote = new Mock<IRemoteCacheProvider>();
        var local = new Mock<ILocalCacheProvider>();
        var behavior = GetBehavior<FakeQuery<string>, string>(new List<ICacheProvider> { local.Object, remote.Object });

        // Act
        var result = await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                RemoteExpires = TimeSpan.FromSeconds(1)
            },
            () => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        result.Should().Be("noCache");
        local.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
        remote.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task CacheBehavior_EnableRemote_HasCache_UseCacheAsync()
    {
        // Arrange
        var remote = new Mock<IRemoteCacheProvider>().AddCacheValue("cacheKey", "cacheValue");
        var local = new Mock<ILocalCacheProvider>();
        var behavior = GetBehavior<FakeQuery<string>, string>(new List<ICacheProvider> { local.Object, remote.Object });

        // Act
        var result = await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                RemoteExpires = TimeSpan.FromSeconds(1)
            },
            () => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        result.Should().Be("cacheValue");
        local.Verify(x => x.GetAsync<string>(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CacheBehavior_ThrowOnGet_ThrowAsync()
    {
        // Arrange
        var remote = new Mock<IRemoteCacheProvider>();
        remote.Setup(x => x.GetAsync<It.IsAnyType>(It.IsAny<string>())).ThrowsAsync(new Exception("test"));
        var behavior = GetBehavior<FakeQuery<string>, string>(
            new List<ICacheProvider>() { remote.Object },
            o => o.ThrowIfFailedOnGet = true);

        // Act
        var act = async () => await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                RemoteExpires = TimeSpan.FromSeconds(1)
            },
            () => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CacheBehavior_ThrowOnGet_NoThrowAsync()
    {
        // Arrange
        var remote = new Mock<IRemoteCacheProvider>();
        remote.Setup(x => x.GetAsync<It.IsAnyType>(It.IsAny<string>())).ThrowsAsync(new Exception("test"));
        var behavior = GetBehavior<FakeQuery<string>, string>(
            new List<ICacheProvider>() { remote.Object },
            o => o.ThrowIfFailedOnGet = false);

        // Act
        var result = await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                RemoteExpires = TimeSpan.FromSeconds(1)
            },
            () => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        result.Should().Be("noCache");
    }

    [Fact]
    public async Task CacheBehavior_ThrowOnUpdate_ThrowAsync()
    {
        // Arrange
        var remote = new Mock<IRemoteCacheProvider>();
        remote.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<It.IsAnyType>(), It.IsAny<TimeSpan>()))
            .ThrowsAsync(new Exception("test"));
        var behavior = GetBehavior<FakeQuery<string>, string>(
            new List<ICacheProvider> { remote.Object },
            o => o.ThrowIfFailedOnUpdate = true);

        // Act
        var act = async () => await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                RemoteExpires = TimeSpan.FromSeconds(1)
            },
            () => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CacheBehavior_NotThrowOnUpdate_NotThrowAsync()
    {
        // Arrange
        var remote = new Mock<IRemoteCacheProvider>();
        remote.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<It.IsAnyType>(), It.IsAny<TimeSpan>()))
            .ThrowsAsync(new Exception("test"));
        var behavior = GetBehavior<FakeQuery<string>, string>(
            new List<ICacheProvider> { remote.Object },
            o => o.ThrowIfFailedOnUpdate = false);

        // Act
        var act = async () => await behavior.Handle(
            new FakeQuery<string>(null, "cacheKey")
            {
                LocalCacheBehavior = CacheBehavior.DisabledCache,
                RemoteCacheBehavior = CacheBehavior.UpdateCacheIfMiss,
                RemoteExpires = TimeSpan.FromSeconds(1)
            },
            () => Task.FromResult("noCache"),
            CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<Exception>();
    }

    [Fact]
    public void CacheBehavior_NoProvider_Throw()
    {
        // Act
        var act = () => GetBehavior<FakeQuery<string>, string>(new List<ICacheProvider>());

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    private static CacheableRequestBehavior<TRequest, TResponse> GetBehavior<TRequest, TResponse>(
        List<ICacheProvider> providers,
        Action<CacheableRequestOptions>? optionConfigure = null)
        where TRequest : ICacheableRequest, IRequest<TResponse>
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