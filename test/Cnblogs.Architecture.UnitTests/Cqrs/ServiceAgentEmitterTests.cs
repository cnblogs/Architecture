using System.Text.RegularExpressions;
using Cnblogs.Architecture.Tool.Generation;
using Cnblogs.Architecture.Tool.Manifest;

namespace Cnblogs.Architecture.UnitTests.Cqrs;

public class ServiceAgentEmitterTests
{
    private static ClrTypeRef Sys(string name) => new() { Namespace = "System", Name = name };

    private static ClrTypeRef Dto(string name) => new() { Namespace = "Cnblogs.Vip.Application.Dto", Name = name };

    private static ClrTypeRef Error(string name) => new() { Namespace = "Cnblogs.Vip.Application.Dto.Errors", Name = name };

    private static ManifestEndpoint Query(
        string route,
        string requestTypeName,
        ResponseShape shape,
        ClrTypeRef responseType,
        List<ManifestParameter> parameters,
        bool enableHead = false,
        List<string>? nullableRoutes = null) =>
        new()
        {
            HttpMethod = "GET",
            HttpMethods = ["GET"],
            Route = route,
            IsQuery = true,
            ResponseShape = shape,
            ResponseType = responseType,
            RequestTypeName = requestTypeName,
            Parameters = parameters,
            EnableHead = enableHead,
            NullableRouteParameters = nullableRoutes ?? []
        };

    private static ManifestEndpoint Command(
        string verb,
        string route,
        string requestTypeName,
        ResponseShape shape,
        ClrTypeRef? responseType,
        ClrTypeRef? payloadType,
        List<ManifestParameter> parameters) =>
        new()
        {
            HttpMethod = verb,
            HttpMethods = [verb],
            Route = route,
            IsQuery = false,
            ResponseShape = shape,
            ResponseType = responseType,
            PayloadType = payloadType,
            RequestTypeName = requestTypeName,
            Parameters = parameters
        };

    private static string EmitClass(params ManifestGroup[] groups)
    {
        var manifest = new EndpointManifest { Groups = groups.ToList() };
        var files = new ServiceAgentEmitter().Emit(manifest, "Cnblogs.Vip.ServiceAgent");
        return files.First(f => f.FileName.EndsWith("Service.cs", StringComparison.Ordinal) && !f.FileName.StartsWith("I", StringComparison.Ordinal) && !f.IsExtensionsFile).Content;
    }

    private static string EmitExtensions(params ManifestGroup[] groups)
    {
        var manifest = new EndpointManifest { Groups = groups.ToList() };
        var files = new ServiceAgentEmitter().Emit(manifest, "Cnblogs.Vip.ServiceAgent");
        return files.First(f => f.IsExtensionsFile).Content;
    }

    private static ManifestParameter Route(string name, ClrTypeRef type, string? token = null, bool nullable = false) =>
        new() { Name = name, Source = ParameterSource.Route, ClrType = type, RouteToken = token ?? name.ToLowerInvariant(), IsNullable = nullable };

    private static ManifestParameter QueryParam(string name, ClrTypeRef type, bool nullable = false, string? defaultLiteral = null, bool hasDefault = false) =>
        new() { Name = name, Source = ParameterSource.Query, ClrType = type, IsNullable = nullable, DefaultValueLiteral = defaultLiteral, HasDefaultValue = hasDefault };

    private static ManifestParameter Body(string name, ClrTypeRef type) =>
        new() { Name = name, Source = ParameterSource.Body, ClrType = type };

    [Fact]
    public void Emit_QueryItem_GeneratesGetItemAsyncCall()
    {
        var cls = EmitClass(new ManifestGroup
        {
            Name = "Vip",
            ErrorType = Error("VipError"),
            Endpoints =
            [
                Query("/api/v1/products/{id:int}", "GetVipProductQuery", ResponseShape.Item, Dto("VipProductDto"), [Route("Id", Sys("Int32"), "id")])
            ]
        });

        Assert.Contains("Task<VipProductDto?> GetVipProductAsync(int id)", cls);
        Assert.Contains("GetItemAsync<VipProductDto>($\"/api/v1/products/{id}\")", cls);
        Assert.Contains(": CqrsServiceAgent<VipError>(httpClient)", cls);
        Assert.Contains("using Cnblogs.Vip.Application.Dto.Errors;", cls);
    }

    [Fact]
    public void Emit_QueryList_GeneratesListItemsAsyncCall()
    {
        var cls = EmitClass(new ManifestGroup
        {
            Name = "Vip",
            ErrorType = Error("VipError"),
            Endpoints = [Query("/api/v1/rules", "ListRulesQuery", ResponseShape.List, new() { Namespace = "System.Collections.Generic", Name = "List", GenericArguments = [Sys("String")] }, [])]
        });

        Assert.Contains("Task<List<string>> ListRulesAsync()", cls);
        Assert.Contains("ListItemsAsync<List<string>>(\"/api/v1/rules\")", cls);
    }

    [Fact]
    public void Emit_QueryPagedList_GeneratesPagedSignature()
    {
        var cls = EmitClass(new ManifestGroup
        {
            Name = "Vip",
            ErrorType = Error("VipError"),
            Endpoints =
            [
                Query(
                    "/api/v1/products",
                    "ListProductsQuery",
                    ResponseShape.PagedList,
                    new() { Namespace = "Cnblogs.Architecture.Ddd.Infrastructure.Abstractions", Name = "PagedList", GenericArguments = [Dto("VipProductDto")] },
                    [QueryParam("PagingParams", new() { Namespace = "Cnblogs.Architecture.Ddd.Infrastructure.Abstractions", Name = "PagingParams" }), QueryParam("OrderByString", Sys("String"), nullable: true)])
            ]
        });

        Assert.Contains("Task<PagedList<VipProductDto>> ListProductsAsync(int? pageIndex = null, int? pageSize = null, string? orderByString = null)", cls);
        Assert.Contains("ListPagedItemsAsync<VipProductDto>(\"/api/v1/products\", pageIndex, pageSize, orderByString)", cls);
    }

    [Fact]
    public void Emit_PostCommandWithResult_GeneratesPostCommandAsync()
    {
        var cls = EmitClass(new ManifestGroup
        {
            Name = "Vip",
            ErrorType = Error("VipError"),
            Endpoints = [Command("POST", "/api/v1/products", "CreateProductCommand", ResponseShape.Item, Dto("VipProductDto"), Dto("CreateProductPayload"), [Body("payload", Dto("CreateProductPayload"))])]
        });

        Assert.Contains("Task<CommandResponse<VipProductDto, VipError>> CreateProductAsync(CreateProductPayload payload)", cls);
        Assert.Contains("PostCommandAsync<VipProductDto, CreateProductPayload>(\"/api/v1/products\", payload)", cls);
    }

    [Fact]
    public void Emit_DeleteCommandWithoutPayload_GeneratesDeleteCommandAsync()
    {
        var cls = EmitClass(new ManifestGroup
        {
            Name = "Vip",
            ErrorType = Error("VipError"),
            Endpoints = [Command("DELETE", "/api/v1/strings/{id:int}", "DeleteStringCommand", ResponseShape.None, null, null, [Route("Id", Sys("Int32"), "id")])]
        });

        Assert.Contains("Task<CommandResponse<VipError>> DeleteStringAsync(int id)", cls);
        Assert.Contains("DeleteCommandAsync($\"/api/v1/strings/{id}\")", cls);
    }

    [Fact]
    public void Emit_NullableRouteExpansion_CollapsesIntoOneMethod()
    {
        var full = "/api/v1/apps/{appId}/strings/{stringId:int}/value";
        var nulled = "/api/v1/apps/-/strings/-/value";
        var nullableRoutes = new List<string> { "appId", "stringId" };

        var cls = EmitClass(new ManifestGroup
        {
            Name = "Vip",
            ErrorType = Error("VipError"),
            Endpoints =
            [
                Query(full, "GetStringQuery", ResponseShape.Item, Sys("String"), [Route("AppId", Sys("String"), "appId", nullable: true), Route("StringId", Sys("Int32"), "stringId", nullable: true)], nullableRoutes: nullableRoutes),
                Query(nulled, "GetStringQuery", ResponseShape.Item, Sys("String"), [Route("AppId", Sys("String"), "appId", nullable: true), Route("StringId", Sys("Int32"), "stringId", nullable: true)], nullableRoutes: nullableRoutes)
            ]
        });

        // Exactly one GetStringAsync method (the two expanded routes collapse).
        Assert.Single(Regex.Matches(cls, "GetStringAsync\\("));
        Assert.Contains("GetStringAsync(string? appId = null, int? stringId = null)", cls);
        Assert.Contains("(appId?.ToString() ?? \"-\")", cls);
        Assert.Contains("(stringId?.ToString() ?? \"-\")", cls);
    }

    [Fact]
    public void Emit_EnableHead_GeneratesHasMethod()
    {
        var cls = EmitClass(new ManifestGroup
        {
            Name = "Vip",
            ErrorType = Error("VipError"),
            Endpoints =
            [
                Query("/api/v1/products/{id:int}", "GetProductQuery", ResponseShape.Item, Dto("VipProductDto"), [Route("Id", Sys("Int32"), "id")], enableHead: true)
            ]
        });

        Assert.Contains("Task<bool> HasProductAsync(int id)", cls);
        Assert.Contains("HasItemAsync($\"/api/v1/products/{id}\")", cls);
    }

    [Fact]
    public void Emit_Extensions_RegistersAllGroups()
    {
        var ext = EmitExtensions(
            new ManifestGroup { Name = "Vip", ErrorType = Error("VipError"), Endpoints = [] },
            new ManifestGroup { Name = "Store", ErrorType = Error("StoreError"), Endpoints = [] });

        Assert.Contains("AddServiceAgent<IVipService, VipService>(baseUri)", ext);
        Assert.Contains("AddServiceAgent<IStoreService, StoreService>(baseUri)", ext);
        Assert.Contains("public static IServiceCollection AddServiceAgents(this IServiceCollection services, string baseUri)", ext);
    }
}
