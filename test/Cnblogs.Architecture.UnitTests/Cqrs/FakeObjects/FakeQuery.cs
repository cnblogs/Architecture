using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

using MediatR;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public class FakeQuery<TResponse> : ICachableRequest, IRequest<TResponse>, IValidatable
{
    private readonly string? _cacheGroupKey;
    private readonly string _cacheKey;

    public FakeQuery()
    {
        _cacheKey = "test";
        ValidateFunction = () => null;
    }

    public FakeQuery(Func<ValidationError?> validateFunction)
        : this()
    {
        ValidateFunction = validateFunction;
    }

    public FakeQuery(string? cacheGroupKey, string cacheKey)
        : this()
    {
        _cacheGroupKey = cacheGroupKey;
        _cacheKey = cacheKey;
    }

    /// <inheritdoc />
    public CacheBehavior LocalCacheBehavior { get; set; }

    /// <inheritdoc />
    public CacheBehavior RemoteCacheBehavior { get; set; }

    /// <inheritdoc />
    public TimeSpan? LocalExpires { get; set; }

    /// <inheritdoc />
    public TimeSpan? RemoteExpires { get; set; }

    public Func<ValidationError?> ValidateFunction { get; set; }

    /// <inheritdoc />
    public string? CacheGroupKey()
    {
        return _cacheGroupKey;
    }

    /// <inheritdoc />
    public object?[] GetCacheKeyParameters()
    {
        return [_cacheKey];
    }

    /// <inheritdoc />
    public void Validate(ValidationErrors validationErrors)
    {
        var error = ValidateFunction.Invoke();
        if (error is not null)
        {
            validationErrors.Add(error);
        }
    }
}