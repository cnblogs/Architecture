using System.Collections;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

/// <summary>
///     Pipeline behavior that enriches DTOs after query execution.
///     Automatically resolves <see cref="IEnricher{T}" /> based on the DTO type in the response.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class EnricherBehavior<TRequest, TResponse>(IServiceProvider sp, EnricherMappingCache enricherMappingCache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);
        if (response is null)
        {
            return response;
        }

        object? itemToMap = response;
        var isEnumerable = false;
        var elementType = typeof(TResponse);

        // 是 CommandResponse<T, TError> 吗？
        if (itemToMap is IObjectResponse objectResponse)
        {
            itemToMap = objectResponse.GetResult();
            if (itemToMap is null)
            {
                return response;
            }

            elementType = itemToMap.GetType();
        }

        // 是 PagedList 吗？
        if (itemToMap is IPagedList pagedList)
        {
            itemToMap = pagedList.GetItems();
            elementType = pagedList.GetType().GetGenericArguments()[0];
            isEnumerable = true;
        }
        else if (itemToMap is IDictionary dictionary)
        {
            var dictionaryType = elementType
                .GetInterfaces()
                .FirstOrDefault(s => s.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            if (dictionaryType is not null)
            {
                isEnumerable = true;
                itemToMap = dictionary.Values;
                elementType = dictionaryType.GetGenericArguments()[1];
            }
        }
        else if (itemToMap is IEnumerable and not string)
        {
            // 是 IEnumerable 吗？
            foreach (var iface in elementType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    elementType = iface.GetGenericArguments()[0];
                    isEnumerable = true;
                }
            }
        }

        // 是值类型或者 string 吗？
        if (elementType.IsValueType || itemToMap is string)
        {
            return response;
        }

        var enricherType = typeof(IEnricher<>).MakeGenericType(elementType);
        var enrichers = sp.GetServices(enricherType)
            .Where(x => x is not null)
            .Select(x => (IEnricher)x!)
            .OrderByDescending(x => x.AllowParallel);
        var parallelTasks = new List<Task>();
        foreach (var enricherObj in enrichers)
        {
            var method = (isEnumerable
                ? enricherType.GetMethod("BulkEnrichAsync")
                : enricherType.GetMethod("EnrichAsync"))!;
            var task = (Task)method.Invoke(enricherObj, [itemToMap, cancellationToken])!;
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
}
