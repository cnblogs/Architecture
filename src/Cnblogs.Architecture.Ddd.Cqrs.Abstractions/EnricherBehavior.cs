using System.Collections;
using System.Diagnostics.CodeAnalysis;
using MediatR;

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
    where TRequest : IEnrichableRequest, IRequest<TResponse>
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

        return await EnrichResponseAsync(response, cancellationToken);
    }

    private async Task<TResponse?> EnrichResponseAsync(
        [DisallowNull] TResponse response,
        CancellationToken cancellationToken)
    {
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

        var typeInfo = cache.GetEnricherTypeInfo(elementType);
        if (sp.GetService(typeInfo.EnrichersServiceType) is not IEnumerable<object> enricherObjects)
        {
            return response;
        }

        var enrichers = enricherObjects
            .Select(x => (IEnricher)x)
            .OrderByDescending(x => x.AllowParallel)
            .ToList();

        var method = isEnumerable ? typeInfo.BulkEnrichMethod : typeInfo.EnrichMethod;
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
        var currentType = item.GetType();
        elementType = currentType;
        isEnumerable = false;
        IEnumerable? currentItems = null;

        for (var depth = 0; depth < MaxUnwrapDepth; depth++)
        {
            var containerInfo = cache.GetContainerInfo(currentType);
            if (!containerInfo.IsEnumerable)
            {
                elementType = containerInfo.ElementType;
                break;
            }

            isEnumerable = true;
            elementType = containerInfo.ElementType;

            currentItems = currentItems is null
                ? containerInfo.ExtractItems!.Invoke(item)
                : Flatten(currentItems, containerInfo);

            currentType = containerInfo.ElementType;
        }

        return isEnumerable ? ExtractNonNulls(currentItems!, elementType) : item;
    }

    private static IEnumerable ExtractNonNulls(IEnumerable items, Type elementType)
    {
        var listType = typeof(List<>).MakeGenericType(elementType);
        var nonNulls = (IList)Activator.CreateInstance(listType)!;
        foreach (var obj in items)
        {
            if (obj is not null)
            {
                nonNulls.Add(obj);
            }
        }

        return nonNulls;
    }

    private static IEnumerable Flatten(object items, ContainerInfo containerInfo)
    {
        var elementType = containerInfo.ElementType;
        var listType = typeof(List<>).MakeGenericType(elementType);
        var flattened = (IList)Activator.CreateInstance(listType)!;
        foreach (var obj in (IEnumerable)items)
        {
            if (obj is null)
            {
                continue;
            }

            foreach (var i in containerInfo.ExtractItems!.Invoke(obj))
            {
                flattened.Add(i);
            }
        }

        return flattened;
    }
}
