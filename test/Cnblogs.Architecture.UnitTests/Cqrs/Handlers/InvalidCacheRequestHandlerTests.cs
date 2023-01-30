using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Handlers;

public class InvalidCacheRequestHandlerTests
{
    [Fact]
    public Task InvalidCache_ThrowOnRemove_ThrowAsync()
    {
        // Arrange
        var provider = new Mock<IRemoteCacheProvider>();
        provider.Setup(x => x.RemoveAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException());
        var handler = CreateInvalidCacheRequestHandler(
            new List<ICacheProvider>() { provider.Object },
            o => o.ThrowIfFailedOnRemove = true);

        // Act
        var act = async () => await handler.Handle(
            new InvalidCacheRequest(new FakeQuery<string>()),
            CancellationToken.None);

        // Assert
        return Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Fact]
    public Task InvalidCache_ThrowOnRemove_NotThrowAsync()
    {
        // Arrange
        var provider = new Mock<IRemoteCacheProvider>();
        provider.Setup(x => x.RemoveAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException());
        var handler = CreateInvalidCacheRequestHandler(
            new List<ICacheProvider>() { provider.Object },
            o => o.ThrowIfFailedOnRemove = false);

        // Act
        var act = async () => await handler.Handle(
            new InvalidCacheRequest(new FakeQuery<string>()),
            CancellationToken.None);

        // Assert
        return act.Should().NotThrowAsync();
    }

    [Fact]
    public Task InvalidCache_ThrowOnRemove_OverrideByRequest_NotThrowAsync()
    {
        // Arrange
        var provider = new Mock<IRemoteCacheProvider>();
        provider.Setup(x => x.RemoveAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException());
        var handler = CreateInvalidCacheRequestHandler(
            new List<ICacheProvider>() { provider.Object },
            o => o.ThrowIfFailedOnRemove = true);

        // Act
        var act = async () => await handler.Handle(
            new InvalidCacheRequest(new FakeQuery<string>(), false, false),
            CancellationToken.None);

        // Assert
        return act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InvalidCache_RemoveCacheAsync()
    {
        // Arrange
        var remote = new Mock<IRemoteCacheProvider>();
        var local = new Mock<ILocalCacheProvider>();
        var handler = CreateInvalidCacheRequestHandler(
            new List<ICacheProvider>() { remote.Object, local.Object },
            o => o.ThrowIfFailedOnRemove = false);

        // Act
        await handler.Handle(
            new InvalidCacheRequest(new FakeQuery<string>()),
            CancellationToken.None);

        // Assert
        local.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
        remote.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task InvalidCache_RemoveGroupCacheAsync()
    {
        // Arrange
        var remote = new Mock<IRemoteCacheProvider>();
        var local = new Mock<ILocalCacheProvider>();
        var handler = CreateInvalidCacheRequestHandler(
            new List<ICacheProvider>() { remote.Object, local.Object },
            o => o.ThrowIfFailedOnRemove = false);

        // Act
        await handler.Handle(
            new InvalidCacheRequest(new FakeQuery<string>("group", "item"), true),
            CancellationToken.None);

        // Assert
        local.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
        local.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<long>()), Times.Once);
        remote.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
        remote.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<long>()), Times.Once);
    }

    [Fact]
    public void InvalidCache_NoProvider_Throw()
    {
        // Act
        var act = () => CreateInvalidCacheRequestHandler(new List<ICacheProvider>());

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    private InvalidCacheRequestHandler CreateInvalidCacheRequestHandler(
        List<ICacheProvider> cacheProviders,
        Action<CacheableRequestOptions>? configure = null)
    {
        var option = new CacheableRequestOptions();
        configure?.Invoke(option);
        return new InvalidCacheRequestHandler(
            cacheProviders,
            new DefaultDateTimeProvider(),
            new OptionsWrapper<CacheableRequestOptions>(option),
            NullLogger<InvalidCacheRequestHandler>.Instance);
    }
}