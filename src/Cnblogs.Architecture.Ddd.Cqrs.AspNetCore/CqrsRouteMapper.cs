using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Extension methods used for register Command and Query endpoint in minimal API.
/// </summary>
public static class CqrsRouteMapper
{
    private static readonly List<Type> QueryTypes = [typeof(IQuery<>), typeof(IListQuery<>)];

    private static readonly List<Type> CommandTypes = [typeof(ICommand<>), typeof(ICommand<,>)];

    private static readonly string[] GetAndHeadMethods = ["GET", "HEAD"];

    private static readonly List<string> PostCommandPrefixes =
    [
        "Create",
        "Add",
        "New"
    ];

    private static readonly List<string> PutCommandPrefixes =
    [
        "Update",
        "Modify",
        "Replace",
        "Alter"
    ];

    private static readonly List<string> DeleteCommandPrefixes =
    [
        "Delete",
        "Remove",
        "Clean",
        "Clear",
        "Purge"
    ];

    /// <summary>
    ///     Map a query API, using GET method. <typeparamref name="T"/> would been constructed from route and query string.
    /// </summary>
    /// <param name="app"><see cref="IApplicationBuilder"/></param>
    /// <param name="route">The route template for API.</param>
    /// <param name="mapNullableRouteParameters">Multiple routes should be mapped when for nullable route parameters.</param>
    /// <param name="nullRouteParameterPattern">Replace route parameter with given string to represent null.</param>
    /// <param name="enableHead">Map HEAD method for the same routes.</param>
    /// <typeparam name="T">The type of the query.</typeparam>
    /// <returns></returns>
    /// <example>
    /// The following code:
    /// <code>
    ///     app.MapQuery&lt;ItemQuery&gt;("apps/{appName}/instance/{instanceId}/roles", true);
    /// </code>
    /// would register following routes:
    /// <code>
    /// apps/-/instance/-/roles
    /// apps/{appName}/instance/-/roles
    /// apps/-/instance/{instanceId}/roles
    /// apps/{appName}/instance/{instanceId}/roles
    /// </code>
    /// </example>
    public static IEndpointConventionBuilder MapQuery<T>(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route,
        MapNullableRouteParameter mapNullableRouteParameters = MapNullableRouteParameter.Disable,
        string nullRouteParameterPattern = "-",
        bool enableHead = false)
    {
        return app.MapQuery(
            route,
            ([AsParameters] T query) => query,
            mapNullableRouteParameters,
            nullRouteParameterPattern,
            enableHead);
    }

    /// <summary>
    ///     Map a query API, using GET method.
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">The route template.</param>
    /// <param name="handler">The delegate that returns a <see cref="IQuery{TView}"/> instance.</param>
    /// <param name="mapNullableRouteParameters">Multiple routes should be mapped when for nullable route parameters.</param>
    /// <param name="nullRouteParameterPattern">Replace route parameter with given string to represent null.</param>
    /// <param name="enableHead">Allow HEAD for the same routes.</param>
    /// <returns></returns>
    /// <example>
    /// The following code:
    /// <code>
    ///     app.MapQuery("apps/{appName}/instance/{instanceId}/roles", (string? appName, string? instanceId) => new ItemQuery(appName, instanceId), true);
    /// </code>
    /// would register following routes:
    /// <code>
    /// apps/-/instance/-/roles
    /// apps/{appName}/instance/-/roles
    /// apps/-/instance/{instanceId}/roles
    /// apps/{appName}/instance/{instanceId}/roles
    /// </code>
    /// </example>
    public static IEndpointConventionBuilder MapQuery(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route,
        Delegate handler,
        MapNullableRouteParameter mapNullableRouteParameters = MapNullableRouteParameter.Disable,
        string nullRouteParameterPattern = "-",
        bool enableHead = false)
    {
        var (queryType, returnType) = EnsureReturnTypeIsQuery(handler);
        if (mapNullableRouteParameters is MapNullableRouteParameter.Disable)
        {
            return MapRoutes(queryType, returnType, route);
        }

        if (string.IsNullOrWhiteSpace(nullRouteParameterPattern))
        {
            throw new ArgumentNullException(
                nameof(nullRouteParameterPattern),
                "argument must not be null or empty");
        }

        var parsedRoute = RoutePatternFactory.Parse(route);
        var context = new NullabilityInfoContext();
        var nullableRouteProperties = queryType.GetProperties()
            .Where(
                p => p.GetMethod != null
                     && p.SetMethod != null
                     && context.Create(p.GetMethod.ReturnParameter).ReadState == NullabilityState.Nullable)
            .ToList();
        var nullableRoutePattern = parsedRoute.Parameters
            .Where(
                x => nullableRouteProperties.Any(
                    y => string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        var subsets = GetNotEmptySubsets(nullableRoutePattern);
        foreach (var subset in subsets)
        {
            var newRoute = subset.Aggregate(
                route,
                (r, x) =>
                {
                    var regex = new Regex("{" + x.Name + "[^}]*?}", RegexOptions.IgnoreCase);
                    return regex.Replace(r, nullRouteParameterPattern);
                });
            MapRoutes(queryType, returnType, newRoute);
        }

        return MapRoutes(queryType, returnType, route);

        IEndpointConventionBuilder MapRoutes(Type query, Type queryFor, string r)
        {
            var endpoint = enableHead ? app.MapMethods(r, GetAndHeadMethods, handler) : app.MapGet(r, handler);
            var builder = endpoint.AddEndpointFilter<QueryEndpointHandler>()
                .Produces(200, queryFor)
                .WithTags("Queries");
            if (query.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>)))
            {
                // may be null
                builder.Produces(404, queryFor);
            }

            return builder;
        }
    }

    /// <summary>
    ///     Map a command API, using different HTTP methods based on prefix. See example for details.
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">The route template.</param>
    /// <typeparam name="T">The type of the command.</typeparam>
    /// <example>
    /// <code>
    ///     app.MapCommand&lt;CreateItemCommand&gt;("/items"); // Starts with 'Create' or 'Add' - POST
    ///     app.MapCommand&lt;UpdateItemCommand&gt;("/items/{id:int}") // Starts with 'Update' or 'Replace' - PUT
    ///     app.MapCommand&lt;DeleteCommand&gt;("/items/{id:int}") // Starts with 'Delete' or 'Remove' - DELETE
    ///     app.MapCommand&lt;ResetItemCommand&gt;("/items/{id:int}:reset) // Others - PUT
    /// </code>
    /// </example>
    /// <returns></returns>
    public static IEndpointConventionBuilder MapCommand<T>(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route)
    {
        var commandTypeName = typeof(T).Name;
        if (PostCommandPrefixes.Any(x => commandTypeName.StartsWith(x)))
        {
            return app.MapPostCommand<T>(route);
        }

        if (PutCommandPrefixes.Any(x => commandTypeName.StartsWith(x)))
        {
            return app.MapPutCommand<T>(route);
        }

        if (DeleteCommandPrefixes.Any(x => commandTypeName.StartsWith(x)))
        {
            return app.MapDeleteCommand<T>(route);
        }

        return app.MapPutCommand<T>(route);
    }

    /// <summary>
    ///     Map a command API, using different method based on type name prefix.
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">The route template.</param>
    /// <param name="handler">The delegate that returns a instance of <see cref="ICommand{TError}"/>.</param>
    /// <returns></returns>
    /// <example>
    /// <code>
    ///     app.MapCommand("/items", () => new CreateItemCommand()); // Starts with 'Create' or 'Add' - POST
    ///     app.MapCommand("/items/{id:int}", (int id) => new UpdateItemCommand(id)) // Starts with 'Update' or 'Replace' - PUT
    ///     app.MapCommand("/items/{id:int}", (int id) => new DeleteItemCommand(id)) // Starts with 'Delete' or 'Remove' - DELETE
    ///     app.MapCommand("/items/{id:int}:reset, (int id) => new ResetItemCommand(id)) // Others - PUT
    /// </code>
    /// </example>
    public static IEndpointConventionBuilder MapCommand(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route,
        Delegate handler)
    {
        var commandTypeName = EnsureReturnTypeIsCommand(handler).CommandType.Name;
        if (PostCommandPrefixes.Any(x => commandTypeName.StartsWith(x)))
        {
            return app.MapPostCommand(route, handler);
        }

        if (PutCommandPrefixes.Any(x => commandTypeName.StartsWith(x)))
        {
            return app.MapPutCommand(route, handler);
        }

        if (DeleteCommandPrefixes.Any(x => commandTypeName.StartsWith(x)))
        {
            return app.MapDeleteCommand(route, handler);
        }

        return app.MapPutCommand(route, handler);
    }

    /// <summary>
    ///     Map a command API, using POST method and get command data from request body.
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">The route template.</param>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    /// <returns></returns>
    public static IEndpointConventionBuilder MapPostCommand<TCommand>(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route)
    {
        return app.MapPostCommand(route, ([FromBody] TCommand command) => command);
    }

    /// <summary>
    ///     Map a command API, using POST method.
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">The route template.</param>
    /// <param name="handler">The delegate that returns a instance of <see cref="ICommand{TError}"/>.</param>
    /// <returns></returns>
    public static IEndpointConventionBuilder MapPostCommand(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route,
        Delegate handler)
    {
        var (commandType, responseType, errorType) = EnsureReturnTypeIsCommand(handler);
        var builder = app.MapPost(route, handler)
            .AddEndpointFilter<CommandEndpointHandler>()
            .AddCommandOpenApiDescriptions(commandType, responseType, errorType);
        return builder;
    }

    /// <summary>
    ///     Map a command API, using PUT method and get command data from request body.
    /// </summary>
    /// <param name="app"><see cref="IEndpointRouteBuilder"/></param>
    /// <param name="route">The route template.</param>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    /// <returns></returns>
    public static IEndpointConventionBuilder MapPutCommand<TCommand>(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route)
    {
        return app.MapPutCommand(route, ([FromBody] TCommand command) => command);
    }

    /// <summary>
    ///     Map a command API, using PUT method.
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">The route template.</param>
    /// <param name="handler">The delegate that returns a instance of <see cref="ICommand{TError}"/>.</param>
    /// <returns></returns>
    public static IEndpointConventionBuilder MapPutCommand(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route,
        Delegate handler)
    {
        var (commandType, responseType, errorType) = EnsureReturnTypeIsCommand(handler);
        return app.MapPut(route, handler).AddEndpointFilter<CommandEndpointHandler>()
            .AddCommandOpenApiDescriptions(commandType, responseType, errorType);
    }

    /// <summary>
    ///     Map a command API, using DELETE method and get command from route/query parameters.
    /// </summary>
    /// <param name="app"><see cref="IEndpointRouteBuilder"/></param>
    /// <param name="route">The route template.</param>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    /// <returns></returns>
    public static IEndpointConventionBuilder MapDeleteCommand<TCommand>(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route)
    {
        return app.MapDeleteCommand(route, ([AsParameters] TCommand command) => command);
    }

    /// <summary>
    ///     Map a command API, using DELETE method.
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">The route template.</param>
    /// <param name="handler">The delegate that returns a instance of <see cref="ICommand{TError}"/>.</param>
    /// <returns></returns>
    public static IEndpointConventionBuilder MapDeleteCommand(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route,
        Delegate handler)
    {
        var (commandType, responseType, errorType) = EnsureReturnTypeIsCommand(handler);
        return app.MapDelete(route, handler).AddEndpointFilter<CommandEndpointHandler>()
            .AddCommandOpenApiDescriptions(commandType, responseType, errorType);
    }

    /// <summary>
    ///     Map prefix to POST method for further MapCommand() calls.
    /// </summary>
    /// <param name="app"><see cref="IEndpointRouteBuilder"/></param>
    /// <param name="prefix">The new prefix.</param>
    public static IEndpointRouteBuilder MapPrefixToPost(this IEndpointRouteBuilder app, string prefix)
    {
        PostCommandPrefixes.Add(prefix);
        return app;
    }

    /// <summary>
    ///     Stop mapping prefix to POST method for further MapCommand() calls.
    /// </summary>
    /// <param name="app"><see cref="IEndpointRouteBuilder"/></param>
    /// <param name="prefix">The new prefix.</param>
    public static IEndpointRouteBuilder StopMappingPrefixToPost(this IEndpointRouteBuilder app, string prefix)
    {
        PostCommandPrefixes.Remove(prefix);
        return app;
    }

    /// <summary>
    ///     Map prefix to PUT method for further MapCommand() calls.
    /// </summary>
    /// <param name="app"><see cref="IEndpointRouteBuilder"/></param>
    /// <param name="prefix">The new prefix.</param>
    public static IEndpointRouteBuilder MapPrefixToPut(this IEndpointRouteBuilder app, string prefix)
    {
        PutCommandPrefixes.Add(prefix);
        return app;
    }

    /// <summary>
    ///     Stop mapping prefix to PUT method for further MapCommand() calls.
    /// </summary>
    /// <param name="app"><see cref="IEndpointRouteBuilder"/></param>
    /// <param name="prefix">The new prefix.</param>
    public static IEndpointRouteBuilder StopMappingPrefixToPut(this IEndpointRouteBuilder app, string prefix)
    {
        PutCommandPrefixes.Remove(prefix);
        return app;
    }

    /// <summary>
    ///     Map prefix to DELETE method for further MapCommand() calls.
    /// </summary>
    /// <param name="app"><see cref="IEndpointRouteBuilder"/></param>
    /// <param name="prefix">The new prefix.</param>
    public static IEndpointRouteBuilder MapPrefixToDelete(this IEndpointRouteBuilder app, string prefix)
    {
        DeleteCommandPrefixes.Add(prefix);
        return app;
    }

    /// <summary>
    ///     Stop mapping prefix to DELETE method for further MapCommand() calls.
    /// </summary>
    /// <param name="app"><see cref="IEndpointRouteBuilder"/></param>
    /// <param name="prefix">The new prefix.</param>
    public static IEndpointRouteBuilder StopMappingPrefixToDelete(this IEndpointRouteBuilder app, string prefix)
    {
        DeleteCommandPrefixes.Remove(prefix);
        return app;
    }

    private static (Type CommandType, Type? ResponseType, Type ErrorType) EnsureReturnTypeIsCommand(Delegate handler)
    {
        var returnType = handler.Method.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            returnType = returnType.GenericTypeArguments.First();
        }

        var commandType = returnType.GetInterfaces().Where(x => x.IsGenericType)
            .FirstOrDefault(x => CommandTypes.Contains(x.GetGenericTypeDefinition()));
        if (commandType == null)
        {
            throw new ArgumentException(
                "handler does not return command, check if delegate returns type that implements ICommand<> or ICommand<,>");
        }

        Type?[] genericParams = commandType.GetGenericArguments();
        if (genericParams.Length == 1)
        {
            genericParams = [null, genericParams[0]];
        }

        return (returnType, genericParams[0], genericParams[1]!);
    }

    private static (Type, Type) EnsureReturnTypeIsQuery(Delegate handler)
    {
        var returnType = handler.Method.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            returnType = returnType.GenericTypeArguments.First();
        }

        var queryInterface = returnType.GetInterfaces().Where(x => x.IsGenericType)
            .FirstOrDefault(x => QueryTypes.Contains(x.GetGenericTypeDefinition()));
        if (queryInterface == null)
        {
            throw new ArgumentException(
                "handler does not return query, check if delegate returns type that implements IQuery<>");
        }

        return (returnType, queryInterface.GenericTypeArguments[0]);
    }

    private static List<T[]> GetNotEmptySubsets<T>(ICollection<T> items)
    {
        var subsetCount = 1 << items.Count;
        var results = new List<T[]>(subsetCount);
        for (var i = 1; i < subsetCount; i++)
        {
            var index = i;
            var subset = items.Where((_, j) => (index & (1 << j)) > 0).ToArray();
            results.Add(subset);
        }

        return results;
    }

    private static RouteHandlerBuilder AddCommandOpenApiDescriptions(
        this RouteHandlerBuilder builder,
        Type commandType,
        Type? responseType,
        Type errorType)
    {
        var commandResponseType = responseType is null
            ? typeof(CommandResponse<>).MakeGenericType(errorType)
            : typeof(CommandResponse<,>).MakeGenericType(responseType, errorType);
        builder.Produces(200, commandResponseType)
            .Produces(400, commandResponseType)
            .WithTags("Commands");
        if (commandType.GetInterfaces().Any(i => i == typeof(ILockableRequest)))
        {
            builder.Produces(429);
        }

        return builder;
    }
}
