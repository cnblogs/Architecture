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
        List<string>? nullableRoutes = null,
        List<string>? apiVersions = null) =>
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
            NullableRouteParameters = nullableRoutes ?? [],
            ApiVersions = apiVersions ?? []
        };

    private static ManifestEndpoint Command(
        string verb,
        string route,
        string requestTypeName,
        ResponseShape shape,
        ClrTypeRef? responseType,
        ClrTypeRef? payloadType,
        List<ManifestParameter> parameters,
        PayloadContract? payloadContract = null,
        List<string>? apiVersions = null) =>
        new()
        {
            HttpMethod = verb,
            HttpMethods = [verb],
            Route = route,
            IsQuery = false,
            ResponseShape = shape,
            ResponseType = responseType,
            PayloadType = payloadType,
            PayloadContract = payloadContract,
            RequestTypeName = requestTypeName,
            Parameters = parameters,
            ApiVersions = apiVersions ?? []
        };

    private static string EmitClass(params ManifestGroup[] groups)
    {
        return EmitClass(new ServiceAgentEmitter(), groups);
    }

    private static string EmitClass(ServiceAgentEmitter emitter, params ManifestGroup[] groups)
    {
        var manifest = new EndpointManifest { Groups = groups.ToList() };
        var files = emitter.Emit(manifest, "Cnblogs.Vip.ServiceAgent");
        return files.First(f => f.FileName.EndsWith("Service.cs", StringComparison.Ordinal) && !f.FileName.StartsWith("I", StringComparison.Ordinal) && !f.IsExtensionsFile).Content;
    }

    private static string EmitExtensions(params ManifestGroup[] groups)
    {
        return EmitExtensions(new ServiceAgentEmitter(), groups);
    }

    private static string EmitExtensions(ServiceAgentEmitter emitter, params ManifestGroup[] groups)
    {
        var manifest = new EndpointManifest { Groups = groups.ToList() };
        var files = emitter.Emit(manifest, "Cnblogs.Vip.ServiceAgent");
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

        // One AddXxxService method per group, each taking a baseUri argument (no --base-url supplied).
        Assert.Contains("public static IServiceCollection AddVipService(this IServiceCollection services, string baseUri)", ext);
        Assert.Contains("public static IServiceCollection AddStoreService(this IServiceCollection services, string baseUri)", ext);
        Assert.Contains("AddServiceAgent<IVipService, VipService>(baseUri)", ext);
        Assert.Contains("AddServiceAgent<IStoreService, StoreService>(baseUri)", ext);
        Assert.DoesNotContain("AddServiceAgents", ext);
    }

    [Fact]
    public void Emit_Extensions_BaseUrl_BakesUrlAndDropsParam()
    {
        var ext = EmitExtensions(
            new ServiceAgentEmitter { BaseUrl = "http://corp_api" },
            new ManifestGroup { Name = "Corp", ErrorType = Error("CorpError"), Endpoints = [] });

        Assert.Contains("public static IServiceCollection AddCorpService(this IServiceCollection services)", ext);
        Assert.Contains("AddServiceAgent<ICorpService, CorpService>(\"http://corp_api\")", ext);
        Assert.DoesNotContain("baseUri", ext);
    }

    [Fact]
    public void Emit_CommandAsBody_GeneratesPayloadPocoInsteadOfCommandType()
    {
        // Arrange — the command lives in the Application layer; the view lives in a contracts layer (a distinct namespace,
        // so we can assert the command's namespace is dropped while the view's is kept).
        var commandType = new ClrTypeRef { Namespace = "Cnblogs.Blog.Application.Commands", Name = "CreateBlogCommand" };
        var viewType = new ClrTypeRef { Namespace = "Cnblogs.Blog.Contracts", Name = "BlogDto" };
        var contract = new PayloadContract
        {
            Properties =
            [
                new PayloadProperty { Name = "Title", ClrType = Sys("String") },
                new PayloadProperty { Name = "Summary", ClrType = Sys("String"), IsNullable = true }
            ]
        };

        // Act
        var files = new ServiceAgentEmitter().Emit(
            new EndpointManifest
            {
                Groups =
                [
                    new ManifestGroup
                    {
                        Name = "Blog",
                        ErrorType = Error("BlogError"),
                        Endpoints =
                        [
                            Command(
                                "POST",
                                "/api/blogs",
                                "CreateBlogCommand",
                                ResponseShape.Item,
                                viewType,
                                commandType,
                                [Body("payload", commandType)],
                                payloadContract: contract)
                        ]
                    }
                ]
            },
            "Cnblogs.Blog.ServiceAgent");

        // Assert — the signature and call use the generated POCO name, not the command type...
        var cls = files.First(f => f.FileName == "BlogService.cs").Content;
        Assert.Contains("Task<CommandResponse<BlogDto, BlogError>> CreateBlogAsync(CreateBlogPayload payload)", cls);
        Assert.Contains("PostCommandAsync<BlogDto, CreateBlogPayload>(\"/api/blogs\", payload)", cls);

        // ...the command's namespace is NOT imported (the client need not reference the Application project)...
        Assert.DoesNotContain("using Cnblogs.Blog.Application.Commands;", cls);

        // ...while the view's namespace still is.
        Assert.Contains("using Cnblogs.Blog.Contracts;", cls);

        // A separate POCO file mirrors the command's settable properties.
        var poco = files.Single(f => f.FileName == "CreateBlogPayload.cs");
        Assert.Contains("public class CreateBlogPayload", poco.Content);
        Assert.Contains("public string Title { get; set; }", poco.Content);
        Assert.Contains("public string? Summary { get; set; }", poco.Content);
    }

    [Fact]
    public void Emit_SameCommandMappedTwice_EmitsSinglePoco()
    {
        // Arrange — the same command type mapped at two routes must share one POCO (a duplicate class declaration
        // would not compile in the shared namespace).
        var commandType = new ClrTypeRef { Namespace = "Cnblogs.Blog.Application.Commands", Name = "CreateBlogCommand" };
        var contract = new PayloadContract
        {
            Properties = [new PayloadProperty { Name = "Title", ClrType = Sys("String") }]
        };

        // Act
        var files = new ServiceAgentEmitter().Emit(
            new EndpointManifest
            {
                Groups =
                [
                    new ManifestGroup
                    {
                        Name = "Blog",
                        ErrorType = Error("BlogError"),
                        Endpoints =
                        [
                            Command("POST", "/api/blogs", "CreateBlogCommand", ResponseShape.None, null, commandType, [Body("payload", commandType)], payloadContract: contract),
                            Command("POST", "/api/v2/blogs", "CreateBlogCommand", ResponseShape.None, null, commandType, [Body("payload", commandType)], payloadContract: contract)
                        ]
                    }
                ]
            },
            "Cnblogs.Blog.ServiceAgent");

        // Assert — exactly one POCO file for the shared command body type.
        Assert.Single(files, f => f.FileName.EndsWith("Payload.cs", StringComparison.Ordinal));
        Assert.Single(files, f => f.FileName == "CreateBlogPayload.cs");
    }

    [Fact]
    public void Emit_SingleApiVersion_KeepsUnsuffixedNameAndStampsVersion()
    {
        // Arrange — every endpoint declares v2: one un-suffixed agent, and the {version:apiVersion} token stamped
        // with the declared version (not the historical default "1").
        var emitter = new ServiceAgentEmitter();

        // Act
        var files = emitter.Emit(
            new EndpointManifest
            {
                Groups =
                [
                    new ManifestGroup
                    {
                        Name = "Accusation",
                        ErrorType = Error("AccusationError"),
                        Endpoints =
                        [
                            Query(
                                "/api/v{version:apiVersion}/accusations",
                                "ListAccusationQuery",
                                ResponseShape.PagedList,
                                new() { Namespace = "Cnblogs.Architecture.Ddd.Infrastructure.Abstractions", Name = "PagedList", GenericArguments = [Dto("AccusationDto")] },
                                [QueryParam("ReporterId", Sys("Guid"), nullable: true)],
                                apiVersions: ["2"])
                        ]
                    }
                ]
            },
            "Cnblogs.Report.ServiceAgent");

        // Assert — the un-suffixed pair exists and the URL uses the declared version.
        Assert.Contains(files, f => f.FileName == "IAccusationService.cs");
        Assert.Contains(files, f => f.FileName == "AccusationService.cs");
        var cls = files.First(f => f.FileName == "AccusationService.cs").Content;
        Assert.Contains("\"/api/v2/accusations\"", cls);
        Assert.DoesNotContain("/api/v1/", cls);
    }

    [Fact]
    public void Emit_MultipleApiVersions_SplitsIntoSuffixedGroups()
    {
        // Arrange — the same group spans v1 and v2 endpoints (the default, no-option path).
        var v1Endpoint = Query(
            "/api/v{version:apiVersion}/accusations/{id:int}",
            "GetAccusationQuery",
            ResponseShape.Item,
            Dto("AccusationDto"),
            [Route("Id", Sys("Int32"), "id")],
            apiVersions: ["1"]);
        var v2Endpoint = Query(
            "/api/v{version:apiVersion}/accusations",
            "ListAccusationQuery",
            ResponseShape.PagedList,
            new() { Namespace = "Cnblogs.Architecture.Ddd.Infrastructure.Abstractions", Name = "PagedList", GenericArguments = [Dto("AccusationDto")] },
            [QueryParam("ReporterId", Sys("Guid"), nullable: true)],
            apiVersions: ["2"]);

        // Act
        var files = new ServiceAgentEmitter().Emit(
            new EndpointManifest
            {
                Groups =
                [
                    new ManifestGroup { Name = "Accusation", ErrorType = Error("AccusationError"), Endpoints = [v1Endpoint, v2Endpoint] }
                ]
            },
            "Cnblogs.Report.ServiceAgent");

        // Assert — one suffixed pair per version, each stamped with its own URL version.
        Assert.Contains(files, f => f.FileName == "IAccusationV1Service.cs");
        Assert.Contains(files, f => f.FileName == "IAccusationV2Service.cs");
        var v1Cls = files.First(f => f.FileName == "AccusationV1Service.cs").Content;
        var v2Cls = files.First(f => f.FileName == "AccusationV2Service.cs").Content;
        Assert.Contains("\"/api/v1/accusations/{id}\"", v1Cls);
        Assert.Contains("\"/api/v2/accusations\"", v2Cls);

        // The DI extensions register each split agent under its suffixed name.
        var ext = files.First(f => f.IsExtensionsFile).Content;
        Assert.Contains("AddAccusationV1Service(", ext);
        Assert.Contains("AddAccusationV2Service(", ext);
    }

    [Fact]
    public void Emit_RequestedApiVersion_FiltersEndpointsAndKeepsUnsuffixedName()
    {
        // Arrange — --api-version 2: only v2 endpoints are emitted, as one un-suffixed IAccusationService.
        var v1Endpoint = Query(
            "/api/v{version:apiVersion}/accusations/{id:int}",
            "GetAccusationQuery",
            ResponseShape.Item,
            Dto("AccusationDto"),
            [Route("Id", Sys("Int32"), "id")],
            apiVersions: ["1"]);
        var v2Endpoint = Query(
            "/api/v{version:apiVersion}/accusations",
            "ListAccusationQuery",
            ResponseShape.PagedList,
            new() { Namespace = "Cnblogs.Architecture.Ddd.Infrastructure.Abstractions", Name = "PagedList", GenericArguments = [Dto("AccusationDto")] },
            [QueryParam("ReporterId", Sys("Guid"), nullable: true)],
            apiVersions: ["2"]);
        var emitter = new ServiceAgentEmitter { RequestedApiVersion = "2" };

        // Act
        var files = emitter.Emit(
            new EndpointManifest
            {
                Groups =
                [
                    new ManifestGroup { Name = "Accusation", ErrorType = Error("AccusationError"), Endpoints = [v1Endpoint, v2Endpoint] }
                ]
            },
            "Cnblogs.Report.ServiceAgent");

        // Assert — the un-suffixed pair (per the requirement), the v1 endpoint dropped with a warning.
        Assert.Contains(files, f => f.FileName == "IAccusationService.cs");
        Assert.DoesNotContain(files, f => f.FileName.EndsWith("V1Service.cs", StringComparison.Ordinal));
        var cls = files.First(f => f.FileName == "AccusationService.cs").Content;
        Assert.Contains("\"/api/v2/accusations\"", cls);
        Assert.DoesNotContain("GetAccusationAsync", cls);
        Assert.Contains(
            emitter.Diagnostics,
            d => d.Contains("not declaring API version '2'", StringComparison.Ordinal));
    }

    [Fact]
    public void Emit_RequestedApiVersion_KeepsUnversionedEndpoints()
    {
        // Arrange — an endpoint without any version metadata (e.g. a plain MapQuery outside versioned groups)
        // stays in the requested-version output; it falls back to the emitter default stamp.
        var unversioned = Query("/api/v{version:apiVersion}/health", "GetHealthQuery", ResponseShape.Item, Sys("String"), [], apiVersions: []);
        var v1Endpoint = Query(
            "/api/v{version:apiVersion}/accusations/{id:int}",
            "GetAccusationQuery",
            ResponseShape.Item,
            Dto("AccusationDto"),
            [Route("Id", Sys("Int32"), "id")],
            apiVersions: ["1"]);
        var emitter = new ServiceAgentEmitter { RequestedApiVersion = "2" };

        // Act
        var files = emitter.Emit(
            new EndpointManifest
            {
                Groups =
                [
                    new ManifestGroup { Name = "Accusation", ErrorType = Error("AccusationError"), Endpoints = [v1Endpoint, unversioned] }
                ]
            },
            "Cnblogs.Report.ServiceAgent");

        // Assert
        var cls = files.First(f => f.FileName == "AccusationService.cs").Content;
        Assert.Contains("GetHealthAsync", cls);
    }

    [Fact]
    public void Emit_RequestedApiVersion_NormalizesMinorVersions()
    {
        // Arrange — "2.0" and "2" are the same Asp.Versioning version; --api-version 2 must match a declared "2.0".
        var endpoint = Query(
            "/api/v{version:apiVersion}/accusations",
            "ListAccusationQuery",
            ResponseShape.PagedList,
            new() { Namespace = "Cnblogs.Architecture.Ddd.Infrastructure.Abstractions", Name = "PagedList", GenericArguments = [Dto("AccusationDto")] },
            [],
            apiVersions: ["2.0"]);
        var emitter = new ServiceAgentEmitter { RequestedApiVersion = "2" };

        // Act
        var files = emitter.Emit(
            new EndpointManifest { Groups = [new ManifestGroup { Name = "Accusation", Endpoints = [endpoint] }] },
            "Cnblogs.Report.ServiceAgent");

        // Assert — the endpoint survives the filter.
        var cls = files.First(f => f.FileName == "AccusationService.cs").Content;
        Assert.Contains("ListAccusationAsync", cls);
    }

    [Fact]
    public void Emit_NoVersionMetadata_DefaultsToVersionOneStamp()
    {
        // Arrange — no endpoint declares versions (plain route groups without HasApiVersion): the legacy behavior
        // (stamp "1") is preserved.
        var cls = EmitClass(
            new ManifestGroup
            {
                Name = "Vip",
                ErrorType = Error("VipError"),
                Endpoints =
                [
                    Query("/api/v{version:apiVersion}/products/{id:int}", "GetVipProductQuery", ResponseShape.Item, Dto("VipProductDto"), [Route("Id", Sys("Int32"), "id")])
                ]
            });

        // Assert
        Assert.Contains("\"/api/v1/products/{id}\"", cls);
    }
}
