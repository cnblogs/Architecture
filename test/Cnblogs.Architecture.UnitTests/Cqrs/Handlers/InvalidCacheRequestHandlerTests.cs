using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Handlers;

public class InvalidCacheRequestHandlerTests
{
    [Fact]
    public Task InvalidCache_ThrowOnRemove_ThrowAsync()
    {
        // Arrange
        var provider = Substitute.For<IRemoteCacheProvider>();
        provider.RemoveAsync(Arg.Any<string>())
            .ThrowsAsync(new InvalidOperationException());
        var handler = CreateInvalidCacheRequestHandler(
            new List<ICacheProvider>() { provider },
            o => o.ThrowIfFailedOnRemove = true);

        // Act
        var act = async () => await handler.Handle(
            new InvalidCacheRequest(new FakeQuery<string>()),
            CancellationToken.None);

        // Assert
        return act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public Task InvalidCache_ThrowOnRemove_NotThrowAsync()
    {
        // Arrange
        var provider = Substitute.For<IRemoteCacheProvider>();
        provider.RemoveAsync(Arg.Any<string>())
            .ThrowsAsync(new InvalidOperationException());
        var handler = CreateInvalidCacheRequestHandler(
            new List<ICacheProvider>() { provider },
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
        var provider = Substitute.For<IRemoteCacheProvider>();
        provider.RemoveAsync(Arg.Any<string>())
            .ThrowsAsync(new InvalidOperationException());
        var handler = CreateInvalidCacheRequestHandler(
            new List<ICacheProvider>() { provider },
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
        var remote = Substitute.For<IRemoteCacheProvider>();
        var local = Substitute.For<ILocalCacheProvider>();
        var handler = CreateInvalidCacheRequestHandler(
            new List<ICacheProvider>() { remote, local },
            o => o.ThrowIfFailedOnRemove = false);

        // Act
        await handler.Handle(
            new InvalidCacheRequest(new FakeQuery<string>()),
            CancellationToken.None);

        // Assert
        await local.Received(1).RemoveAsync(Arg.Any<string>());
        await remote.Received(1).RemoveAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task InvalidCache_RemoveGroupCacheAsync()
    {
        // Arrange
        var remote = Substitute.For<IRemoteCacheProvider>();
        var local = Substitute.For<ILocalCacheProvider>();
        var handler = CreateInvalidCacheRequestHandler(
            new List<ICacheProvider>() { remote, local },
            o => o.ThrowIfFailedOnRemove = false);

        // Act
        await handler.Handle(
            new InvalidCacheRequest(new FakeQuery<string>("group", "item"), true),
            CancellationToken.None);

        // Assert
        await local.Received(1).RemoveAsync(Arg.Any<string>());
        await local.Received(1).UpdateAsync(Arg.Any<string>(), Arg.Any<long>());
        await remote.Received(1).RemoveAsync(Arg.Any<string>());
        await remote.Received(1).UpdateAsync(Arg.Any<string>(), Arg.Any<long>());
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
