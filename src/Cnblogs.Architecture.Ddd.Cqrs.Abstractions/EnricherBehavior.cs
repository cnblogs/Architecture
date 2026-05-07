using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Cnblogs.Architecture.Ddd.Cqrs.Abstractions.Internals;
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

        var enricherPlan = cache.GetEnricherPlan(elementType);
        if (enricherPlan is null)
        {
            return response;
        }

        foreach (var stage in enricherPlan)
        {
            await RunEnrichStageAsync(stage, isEnumerable, toEnrich, cancellationToken);
        }

        return response;
    }

    private async Task RunEnrichStageAsync(
        EnricherStage stage,
        bool isEnumerable,
        object toEnrich,
        CancellationToken cancellationToken)
    {
        var parallelTasks = new List<Task>();

        foreach (var descriptor in stage.Entries)
        {
            var enricherObj = sp.GetService(descriptor.ImplType);
            if (enricherObj is null)
            {
                continue;
            }

            var method = isEnumerable ? descriptor.BulkEnrichMethod : descriptor.EnrichMethod;
            var task = (Task)method.Invoke(enricherObj, [toEnrich, cancellationToken])!;

            if (((IEnricher)enricherObj).AllowParallel)
            {
                parallelTasks.Add(task);
            }
            else
            {
                await task;
            }
        }

        await Task.WhenAll(parallelTasks);
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