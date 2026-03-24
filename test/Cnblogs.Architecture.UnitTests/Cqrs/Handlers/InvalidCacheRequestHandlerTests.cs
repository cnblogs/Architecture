using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Handlers;

public class InvalidCacheRequestHandlerTests
{
    [Fact]
    public async Task InvalidCache_ThrowOnRemove_NotThrowAsync()
    {
        // Arrange
        var provider = Substitute.For<ICacheProvider>();
        provider.RemoveAsync(Arg.Any<string>())
            .Throws(new InvalidOperationException());
        var handler = CreateInvalidCacheRequestHandler(provider);

        // Act
        await handler.Handle(
            new InvalidCacheRequest(new FakeQuery<string>()),
            CancellationToken.None);

        // Assert-Not throws.
    }

    [Fact]
    public async Task InvalidCache_ThrowOnRemove_CatchExceptionAsync()
    {
        // Arrange
        var provider = Substitute.For<ICacheProvider>();
        provider.RemoveAsync(Arg.Any<string>())
            .Throws(new InvalidOperationException());
        var handler = CreateInvalidCacheRequestHandler(provider);

        // Act
        await handler.Handle(
            new InvalidCacheRequest(new FakeQuery<string>(), false, false),
            CancellationToken.None);

        // Assert-Not throws
    }

    [Fact]
    public async Task InvalidCache_RemoveCacheAsync()
    {
        // Arrange
        var remote = Substitute.For<ICacheProvider>();
        var handler = CreateInvalidCacheRequestHandler(remote);

        // Act
        await handler.Handle(
            new InvalidCacheRequest(new FakeQuery<string>()),
            CancellationToken.None);

        // Assert
        await remote.Received(1).RemoveAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task InvalidCache_RemoveGroupCacheAsync()
    {
        // Arrange
        var remote = Substitute.For<ICacheProvider>();
        var handler = CreateInvalidCacheRequestHandler(remote);

        // Act
        await handler.Handle(
            new InvalidCacheRequest(new FakeQuery<string>("group", "item"), true),
            CancellationToken.None);

        // Assert
        await remote.Received(1).RemoveGroupAsync(Arg.Any<string>());
    }

    private InvalidCacheRequestHandler CreateInvalidCacheRequestHandler(ICacheProvider cacheProvider)
    {
        return new InvalidCacheRequestHandler(
            cacheProvider,
            NullLogger<InvalidCacheRequestHandler>.Instance);
    }
}
