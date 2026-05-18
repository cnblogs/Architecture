using MediatR;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
/// Invalid cache groups by keys
/// </summary>
/// <param name="GroupKeys">Keys of cache groups to invalid.</param>
/// <param name="ThrowIfFailed">Throw exceptions if invalid cache failed.</param>
public record InvalidCacheGroupsRequest(string[] GroupKeys, bool? ThrowIfFailed = null) : IRequest;
