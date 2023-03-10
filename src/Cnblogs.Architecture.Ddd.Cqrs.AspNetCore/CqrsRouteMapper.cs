using System.Diagnostics.CodeAnalysis;

using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Extension methods used for register Command and Query endpoint in minimal API.
/// </summary>
public static class CqrsRouteMapper
{
    private static readonly List<Type> QueryTypes = new() { typeof(IQuery<>), typeof(IListQuery<>) };

    private static readonly List<Type> CommandTypes = new() { typeof(ICommand<>), typeof(ICommand<,>) };

    /// <summary>
    ///     Map a query API, using GET method. <typeparamref name="T"/> would been constructed from route and query string.
    /// </summary>
    /// <param name="app"><see cref="IApplicationBuilder"/></param>
    /// <param name="route">The route template for API.</param>
    /// <typeparam name="T">The type of the query.</typeparam>
    /// <returns></returns>
    public static IEndpointConventionBuilder MapQuery<T>(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route)
    {
        return app.MapQuery(route, ([AsParameters] T query) => query);
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
        return app.MapCommand(route, ([AsParameters] T command) => command);
    }

    /// <summary>
    ///     Map a query API, using GET method.
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">The route template.</param>
    /// <param name="handler">The delegate that returns a <see cref="IQuery{TView}"/> instance.</param>
    /// <returns></returns>
    public static IEndpointConventionBuilder MapQuery(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route,
        Delegate handler)
    {
        var isQuery = handler.Method.ReturnType.GetInterfaces().Where(x => x.IsGenericType)
            .Any(x => QueryTypes.Contains(x.GetGenericTypeDefinition()));
        if (isQuery == false)
        {
            throw new ArgumentException(
                "delegate does not return a query, please make sure it returns object that implement IQuery<> or IListQuery<> or interface that inherit from them");
        }

        return app.MapGet(route, handler).AddEndpointFilter<QueryEndpointHandler>();
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
        EnsureDelegateReturnTypeIsCommand(handler);
        var commandTypeName = handler.Method.ReturnType.Name;
        if (commandTypeName.StartsWith("Create") || commandTypeName.StartsWith("Add"))
        {
            return app.MapPostCommand(route, handler);
        }

        if (commandTypeName.StartsWith("Update") || commandTypeName.StartsWith("Replace"))
        {
            return app.MapPutCommand(route, handler);
        }

        if (commandTypeName.StartsWith("Delete") || commandTypeName.StartsWith("Remove"))
        {
            return app.MapDeleteCommand(route, handler);
        }

        return app.MapPutCommand(route, handler);
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
        EnsureDelegateReturnTypeIsCommand(handler);
        return app.MapPost(route, handler).AddEndpointFilter<CommandEndpointHandler>();
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
        EnsureDelegateReturnTypeIsCommand(handler);
        return app.MapPut(route, handler).AddEndpointFilter<CommandEndpointHandler>();
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
        EnsureDelegateReturnTypeIsCommand(handler);
        return app.MapDelete(route, handler).AddEndpointFilter<CommandEndpointHandler>();
    }

    private static void EnsureDelegateReturnTypeIsCommand(Delegate handler)
    {
        var isCommand = handler.Method.ReturnType.GetInterfaces().Where(x => x.IsGenericType)
            .Any(x => CommandTypes.Contains(x.GetGenericTypeDefinition()));
        if (isCommand == false)
        {
            throw new ArgumentException(
                "handler does not return command, check if delegate returns type that implements ICommand<> or ICommand<,>");
        }
    }
}
