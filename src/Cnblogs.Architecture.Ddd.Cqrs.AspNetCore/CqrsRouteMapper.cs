using System.Diagnostics.CodeAnalysis;

using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     用于 Minimum API CQRS 路径注册的扩展方法。
/// </summary>
public static class CqrsRouteMapper
{
    private static readonly List<Type> QueryTypes = new() { typeof(IQuery<>), typeof(IListQuery<>) };

    private static readonly List<Type> CommandTypes = new() { typeof(ICommand<>), typeof(ICommand<,>) };

    /// <summary>
    ///     添加查询 API，使用 GET 方法访问，参数将自动从路径或查询字符串获取。
    /// </summary>
    /// <param name="app"><see cref="IApplicationBuilder"/></param>
    /// <param name="route">路径模板。</param>
    /// <typeparam name="T">查询类型。</typeparam>
    /// <returns></returns>
    public static IEndpointConventionBuilder MapQuery<T>(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route)
    {
        return app.MapQuery(route, ([AsParameters] T query) => query);
    }

    /// <summary>
    ///     添加一个命令 API，根据前缀选择 HTTP Method，错误会被自动处理。
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">路径模板。</param>
    /// <typeparam name="T">命令类型。</typeparam>
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
    ///     添加一个命令 API，根据前缀选择 HTTP Method，错误会被自动处理。
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">路径模板。</param>
    /// <param name="handler">返回 <typeparamref name="T"/> 的委托。</param>
    /// <typeparam name="T">命令类型。</typeparam>
    /// <example>
    /// <code>
    ///     app.MapCommand&lt;CreateItemCommand&gt;("/items"); // Starts with 'Create' or 'Add' - POST
    ///     app.MapCommand&lt;UpdateItemCommand&gt;("/items/{id:int}") // Starts with 'Update' or 'Replace' - PUT
    ///     app.MapCommand&lt;DeleteCommand&gt;("/items/{id:int}") // Starts with 'Delete' or 'Remove' - DELETE
    ///     app.MapCommand&lt;ResetItemCommand&gt;("/items/{id:int}:reset) // Others - PUT
    /// </code>
    /// </example>
    /// <returns></returns>
    // ReSharper disable once UnusedTypeParameter
    public static IEndpointConventionBuilder MapCommand<T>(
        this IEndpointRouteBuilder app,
        [StringSyntax("Route")] string route,
        Delegate handler)
    {
        return app.MapCommand(route, handler);
    }

    /// <summary>
    ///     添加一个查询 API，使用 GET 方法访问。
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">路径模板。</param>
    /// <param name="handler">构造查询的方法，需要返回 <see cref="IQuery{TView}"/> 的对象。</param>
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
    ///     添加一个命令 API，根据前缀选择 HTTP Method，错误会被自动处理。
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">路径模板。</param>
    /// <param name="handler">构造命令的方法，需要返回 <see cref="ICommand{TError}"/> 类型的对象。</param>
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
    ///     添加一个命令 API，使用 POST 方法访问，错误会被自动处理。
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">路径模板。</param>
    /// <param name="handler">构造命令的方法，需要返回 <see cref="ICommand{TError}"/> 类型的对象。</param>
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
    ///     添加一个命令 API，使用 PUT 方法访问，错误会被自动处理。
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">路径模板。</param>
    /// <param name="handler">构造命令的方法，需要返回 <see cref="ICommand{TError}"/> 类型的对象。</param>
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
    ///     添加一个命令 API，使用 DELETE 方法访问，错误会被自动处理。
    /// </summary>
    /// <param name="app"><see cref="ApplicationBuilder"/></param>
    /// <param name="route">路径模板。</param>
    /// <param name="handler">构造命令的方法，需要返回 <see cref="ICommand{TError}"/> 类型的对象。</param>
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