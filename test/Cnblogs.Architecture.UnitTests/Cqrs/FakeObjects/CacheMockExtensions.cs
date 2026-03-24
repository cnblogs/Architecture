using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public static class CacheMockExtensions
{
    public static ICacheProvider MockCacheValue<T>(this ICacheProvider mock, string key, T value)
    {
        mock.GetOrCreateAsync(
                key.ToLower(),
                Arg.Any<Func<CancellationToken, ValueTask<T>>>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(value));
        return mock;
    }
}
