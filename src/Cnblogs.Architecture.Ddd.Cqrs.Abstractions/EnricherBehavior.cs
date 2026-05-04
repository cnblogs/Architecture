using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Pipeline behavior that enriches DTOs after query execution.
///     Automatically resolves <see cref="IEnricher{T}" /> based on the DTO type in the response.
///     Supports nested containers like <c>PagedList&lt;Dictionary&lt;K,V&gt;&gt;</c>.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class EnricherBehavior<TRequest, TResponse>(IServiceProvider sp, EnricherMappingCache cache)
    : IPipelineBehavior<TRequest, TResponse?>
    where TRequest : IEnrichableRequest
    where TResponse : class
{
    private const int MaxUnwrapDepth = 3;

    /// <inheritdoc />
    public async Task<TResponse?> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse?> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);
        if (response is null || request.IsEnrichSkipped())
        {
            return response;
        }

        var item = UnwrapObjectResponse(response);
        if (item is null)
        {
            return response;
        }

        var toEnrich = UnwrapContainers(item, out var elementType, out var isEnumerable);
        if (elementType.IsValueType || elementType == typeof(string))
        {
            return response;
        }

        var enricherType = typeof(IEnricher<>).MakeGenericType(elementType);
        var enrichers = sp.GetServices(enricherType)
            .Where(x => x is not null)
            .Select(x => (IEnricher)x!)
            .OrderByDescending(x => x.AllowParallel)
            .ToList();

        if (enrichers.Count == 0)
        {
            return response;
        }

        var method = enricherType.GetMethod(isEnumerable ? "BulkEnrichAsync" : "EnrichAsync")!;

        var parallelTasks = new List<Task>();
        foreach (var enricherObj in enrichers)
        {
            var task = (Task)method.Invoke(enricherObj, [toEnrich, cancellationToken])!;
            if (enricherObj.AllowParallel)
            {
                parallelTasks.Add(task);
            }
            else
            {
                await task;
            }
        }

        await Task.WhenAll(parallelTasks);
        return response;
    }

    private static object? UnwrapObjectResponse(object response)
    {
        var item = response;
        if (item is IObjectResponse objectResponse)
        {
            item = objectResponse.GetResult();
        }

        return item;
    }

    private object UnwrapContainers(object item, out Type elementType, out bool isEnumerable)
    {
        elementType = item.GetType();
        isEnumerable = false;
        object? currentItems = null;

        for (var depth = 0; depth < MaxUnwrapDepth; depth++)
        {
            var containerInfo = cache.GetContainerInfo(elementType);
            if (containerInfo is null)
            {
                break;
            }

            isEnumerable = true;
            elementType = containerInfo.ElementType;

            currentItems = currentItems is null
                ? containerInfo.ExtractItems(item)
                : Flatten(currentItems, containerInfo);
        }

        return isEnumerable ? currentItems! : item;
    }

    private static List<object?> Flatten(object items, ContainerInfo containerInfo)
    {
        var flattened = new List<object?>();
        foreach (var obj in (System.Collections.IEnumerable)items)
        {
            if (obj is null)
                continue;
            foreach (var i in containerInfo.ExtractItems(obj))
            {
                flattened.Add(i);
            }
        }

        return flattened;
    }
}
