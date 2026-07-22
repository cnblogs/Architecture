using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cnblogs.Architecture.UnitTests.Cqrs;

public class CqrsEndpointDescriptorBuilderTests
{
    public record SampleDto(int Id, string Title);

    public record SingleQuery(string? AppId = null, int? StringId = null, bool Found = true)
        : IQuery<string>;

    public record StringListQuery : IListQuery<List<string>>;

    public record PageableSampleQuery : IPageableQuery<SampleDto>
    {
        public PagingParams? PagingParams { get; init; }
        public string? OrderByString { get; init; }
    }

    public record CreatePayload(string Title);
    public record UpdatePayload(bool NeedError);

    public class TestError : Enumeration
    {
        public static readonly TestError None = new(0, nameof(None));
        public static readonly TestError Invalid = new(1, nameof(Invalid));

        public TestError(int id, string name)
            : base(id, name)
        {
        }
    }

    public record CreateCommand : ICommand<string, TestError>
    {
        public bool ValidateOnly => false;
    }

    public record UpdateCommand : ICommand<TestError>
    {
        public bool ValidateOnly => false;
    }

    public record CreateCommandFromPayload(CreatePayload Payload) : ICommand<string, TestError>
    {
        public bool ValidateOnly => false;
    }

    public record NoResultCommand : ICommand<TestError>
    {
        public bool ValidateOnly => false;
    }

    public record DeleteCommand(int Id, bool NeedError, bool ValidateOnly = false) : ICommand<string, TestError>;

    public class DummyDependency;

    [Fact]
    public void Build_AsParametersQuery_ExpandsRouteAndQueryParameters()
    {
        // Arrange
        static SingleQuery Handler([AsParameters] SingleQuery q) => q;

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "GET",
            "apps/{appId}/strings/{stringId:int}/value",
            isQuery: true,
            typeof(SingleQuery),
            typeof(string),
            errorType: null,
            mapNullableRouteParametersEnabled: true);

        // Assert
        Assert.Equal("GET", descriptor.HttpMethod);
        Assert.Equal("apps/{appId}/strings/{stringId:int}/value", descriptor.RelativeRoute);
        Assert.True(descriptor.IsQuery);
        Assert.Equal(typeof(SingleQuery), descriptor.RequestType);
        Assert.Equal(typeof(string), descriptor.ResponseType);
        Assert.Null(descriptor.ErrorType);
        Assert.Equal(ResponseShape.Item, descriptor.ResponseShape);

        var appId = Single(descriptor.Parameters, p => p.Name == "AppId");
        Assert.Equal(ParameterSource.Route, appId.Source);
        Assert.Equal(typeof(string), appId.ClrType);
        Assert.True(appId.IsNullable);
        Assert.Equal("appId", appId.RouteToken);

        var stringId = Single(descriptor.Parameters, p => p.Name == "StringId");
        Assert.Equal(ParameterSource.Route, stringId.Source);
        Assert.Equal(typeof(int?), stringId.ClrType);
        Assert.True(stringId.IsNullable);
        Assert.Equal("stringId", stringId.RouteToken);

        var found = Single(descriptor.Parameters, p => p.Name == "Found");
        Assert.Equal(ParameterSource.Query, found.Source);
        Assert.Equal(typeof(bool), found.ClrType);
        Assert.False(found.IsNullable);
        Assert.Null(found.RouteToken);

        Assert.Equivalent(new[] { "AppId", "StringId" }, descriptor.NullableRouteParameters);
    }

    [Fact]
    public void Build_DelegateQuery_ScalarRouteFromTokenAndQueryDefaultKept()
    {
        // Arrange
        static SingleQuery Handler(int stringId, [FromQuery] bool found = true) => default!;

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "GET",
            "strings/{stringId:int}",
            isQuery: true,
            typeof(SingleQuery),
            typeof(string),
            errorType: null);

        // Assert
        var stringIdParam = Single(descriptor.Parameters, p => p.Name == "stringId");
        Assert.Equal(ParameterSource.Route, stringIdParam.Source);
        Assert.Equal(typeof(int), stringIdParam.ClrType);
        Assert.Equal("stringId", stringIdParam.RouteToken);

        var found = Single(descriptor.Parameters, p => p.Name == "found");
        Assert.Equal(ParameterSource.Query, found.Source);
        Assert.True(found.HasDefaultValue);
        Assert.Equal(true, found.DefaultValue);
    }

    [Fact]
    public void Build_ListQueryResponseType_ResponseShapeIsList()
    {
        // Arrange
        static StringListQuery Handler() => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "GET",
            "strings",
            isQuery: true,
            typeof(StringListQuery),
            typeof(List<string>),
            errorType: null);

        // Assert
        Assert.Equal(ResponseShape.List, descriptor.ResponseShape);
    }

    [Fact]
    public void Build_PagedListQueryResponseType_ResponseShapeIsPagedList()
    {
        // Arrange
        static PageableSampleQuery Handler() => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "GET",
            "samples",
            isQuery: true,
            typeof(PageableSampleQuery),
            typeof(PagedList<SampleDto>),
            errorType: null);

        // Assert
        Assert.Equal(ResponseShape.PagedList, descriptor.ResponseShape);
    }

    [Fact]
    public void Build_PostCommandWithExplicitBody_ItemShapeAndBodyPayload()
    {
        // Arrange
        static CreateCommand Handler([FromBody] CreateCommand command) => command;

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "POST",
            "items",
            isQuery: false,
            typeof(CreateCommand),
            typeof(string),
            typeof(TestError));

        // Assert
        Assert.False(descriptor.IsQuery);
        Assert.Equal(ResponseShape.Item, descriptor.ResponseShape);
        Assert.Equal(typeof(CreateCommand), descriptor.PayloadType);
        Assert.Equal(typeof(TestError), descriptor.ErrorType);

        var body = Single(descriptor.Parameters, p => p.Source == ParameterSource.Body);
        Assert.Equal("command", body.Name);
        Assert.Equal(typeof(CreateCommand), body.ClrType);
    }

    [Fact]
    public void Build_NoResultCommand_NullResponseTypeMeansNoneShape()
    {
        // Arrange
        static NoResultCommand Handler() => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "DELETE",
            "items/{id:int}",
            isQuery: false,
            typeof(NoResultCommand),
            responseType: null,
            typeof(TestError));

        // Assert
        Assert.Equal(ResponseShape.None, descriptor.ResponseShape);
        Assert.Null(descriptor.ResponseType);
        Assert.Null(descriptor.PayloadType);
    }

    [Fact]
    public void Build_DelegateCommandImplicitBody_SingleComplexParameterBoundFromBody()
    {
        // Arrange
        static CreateCommandFromPayload Handler(CreatePayload p) => new(p);

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "POST",
            "x",
            isQuery: false,
            typeof(CreateCommandFromPayload),
            typeof(string),
            typeof(TestError));

        // Assert
        var body = Single(descriptor.Parameters, p => p.Source == ParameterSource.Body);
        Assert.Equal("p", body.Name);
        Assert.Equal(typeof(CreatePayload), body.ClrType);
        Assert.Equal(typeof(CreatePayload), descriptor.PayloadType);
    }

    [Fact]
    public void Build_DelegateCommandMixedRouteAndBody_SplitsByTokenMatch()
    {
        // Arrange
        static UpdateCommand Handler(int id, UpdatePayload p) => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "PUT",
            "x/{id:int}",
            isQuery: false,
            typeof(UpdateCommand),
            responseType: null,
            typeof(TestError));

        // Assert
        var id = Single(descriptor.Parameters, p => p.Name == "id");
        Assert.Equal(ParameterSource.Route, id.Source);
        Assert.Equal("id", id.RouteToken);

        var body = Single(descriptor.Parameters, p => p.Source == ParameterSource.Body);
        Assert.Equal("p", body.Name);
        Assert.Equal(typeof(UpdatePayload), body.ClrType);
        Assert.Equal(typeof(UpdatePayload), descriptor.PayloadType);
    }

    [Fact]
    public void Build_ExplicitBindingAttributes_AttributeSourceWins()
    {
        // Arrange
        static object Handler([FromRoute] int scalar, [FromBody] int body) => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "POST",
            "x",
            isQuery: false,
            typeof(object),
            responseType: null,
            typeof(TestError));

        // Assert — attribute wins over the simple-type token inference even when there is no token match.
        var route = Single(descriptor.Parameters, p => p.Name == "scalar");
        Assert.Equal(ParameterSource.Route, route.Source);
        Assert.Equal("scalar", route.RouteToken);

        var body = Single(descriptor.Parameters, p => p.Name == "body");
        Assert.Equal(ParameterSource.Body, body.Source);
        Assert.Null(body.RouteToken);
    }

    [Fact]
    public void Build_FromRouteAttributeForcesRoute_EvenWithoutToken()
    {
        // Arrange
        static object Handler([FromRoute] string name) => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "GET",
            "items",
            isQuery: true,
            typeof(object),
            typeof(string),
            errorType: null);

        // Assert
        var name = Single(descriptor.Parameters, p => p.Name == "name");
        Assert.Equal(ParameterSource.Route, name.Source);
        Assert.Equal("name", name.RouteToken);
    }

    [Fact]
    public void Build_EnableHeadTrue_FlagCopiedToDescriptor()
    {
        // Arrange
        static SingleQuery Handler() => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "GET",
            "x",
            isQuery: true,
            typeof(SingleQuery),
            typeof(string),
            errorType: null,
            enableHead: true);

        // Assert
        Assert.True(descriptor.EnableHead);
    }

    [Fact]
    public void Build_RouteTokenWithConstraint_TokenNameExtractedWithoutConstraint()
    {
        // Arrange — {id:int} should yield token name "id".
        static object Handler(int id) => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "GET",
            "items/{id:int}",
            isQuery: true,
            typeof(object),
            typeof(string),
            errorType: null);

        // Assert
        var id = Single(descriptor.Parameters, p => p.Name == "id");
        Assert.Equal(ParameterSource.Route, id.Source);
        Assert.Equal("id", id.RouteToken);
    }

    [Fact]
    public void Build_NullableOfTParameter_TreatedAsSimpleType()
    {
        // Arrange — int? should unwrap to int inside IsSimpleType, so with no matching token it falls back to Query.
        static object Handler(int? x) => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "GET",
            "y",
            isQuery: true,
            typeof(object),
            typeof(string),
            errorType: null);

        // Assert
        var x = Single(descriptor.Parameters);
        Assert.Equal(ParameterSource.Query, x.Source);
        Assert.Equal(typeof(int?), x.ClrType);
        Assert.True(x.IsNullable);
    }

    [Fact]
    public void Build_CommandResponseTypeIsListShape_StillItemBecauseNotQuery()
    {
        // Arrange — commands always produce Item shape (a single CommandResponse), even when the view is a list.
        static object Handler() => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "POST",
            "x",
            isQuery: false,
            typeof(object),
            typeof(List<string>),
            typeof(TestError));

        // Assert
        Assert.Equal(ResponseShape.Item, descriptor.ResponseShape);
    }

    [Fact]
    public void Build_ArrayQueryResponseType_ResponseShapeIsList()
    {
        // Arrange
        static object Handler() => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "GET",
            "x",
            isQuery: true,
            typeof(object),
            typeof(string[]),
            errorType: null);

        // Assert
        Assert.Equal(ResponseShape.List, descriptor.ResponseShape);
    }

    [Fact]
    public void Build_AsyncHandlerWithCancellationToken_OmitsSpecialTypeParam()
    {
        // Arrange — CancellationToken is server-injected and must never appear in the client signature
        // (regression: it was previously misclassified as an implicit Body parameter).
        static CreateCommand Handler(CreatePayload p, CancellationToken ct) => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "POST",
            "items",
            isQuery: false,
            typeof(CreateCommand),
            typeof(string),
            typeof(TestError));

        // Assert
        Assert.Single(descriptor.Parameters);
        var body = Single(descriptor.Parameters, p => p.Source == ParameterSource.Body);
        Assert.Equal("p", body.Name);
        Assert.Equal(typeof(CreatePayload), descriptor.PayloadType);
        Assert.DoesNotContain(descriptor.Parameters, p => p.ClrType == typeof(CancellationToken));
    }

    [Fact]
    public void Build_FromServiceParameter_OmittedFromSignature()
    {
        // Arrange — a [FromServices] dependency is not part of the wire contract.
        static CreateCommand Handler([FromServices] DummyDependency dep, CreatePayload p) => new();

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "POST",
            "items",
            isQuery: false,
            typeof(CreateCommand),
            typeof(string),
            typeof(TestError));

        // Assert
        Assert.DoesNotContain(descriptor.Parameters, p => p.Name == "dep");
        Assert.Equal(typeof(CreatePayload), descriptor.PayloadType);
    }

    [Fact]
    public void Build_AsParametersDeleteCommand_ExpandsRouteAndQueryWithDefaults()
    {
        // Arrange — mirrors MapDeleteCommand<T> which synthesizes ([AsParameters] T c) => c.
        static DeleteCommand Handler([AsParameters] DeleteCommand c) => c;

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "DELETE",
            "items/{id:int}",
            isQuery: false,
            typeof(DeleteCommand),
            typeof(string),
            typeof(TestError));

        // Assert
        Assert.Equal("DELETE", descriptor.HttpMethod);
        Assert.Null(descriptor.PayloadType);
        Assert.Equal(ResponseShape.Item, descriptor.ResponseShape);

        var id = Single(descriptor.Parameters, p => p.Name == "Id");
        Assert.Equal(ParameterSource.Route, id.Source);
        Assert.Equal("id", id.RouteToken);

        var needError = Single(descriptor.Parameters, p => p.Name == "NeedError");
        Assert.Equal(ParameterSource.Query, needError.Source);

        var validateOnly = Single(descriptor.Parameters, p => p.Name == "ValidateOnly");
        Assert.Equal(ParameterSource.Query, validateOnly.Source);
        // Record positional defaults (ValidateOnly = false) are not visible at the property level and, like
        // RequestDelegateFactory, are not reported as parameter defaults.
        Assert.False(validateOnly.HasDefaultValue);
    }

    [Fact]
    public void Build_NullableRouteParametersEmpty_WhenExpansionDisabled()
    {
        // Arrange — without MapNullableRouteParameter.Enable the mapper registers a single route, so the
        // descriptor must not advertise any nullable route parameters.
        static SingleQuery Handler([AsParameters] SingleQuery q) => q;

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "GET",
            "apps/{appId}/strings/{stringId:int}/value",
            isQuery: true,
            typeof(SingleQuery),
            typeof(string),
            errorType: null,
            mapNullableRouteParametersEnabled: false);

        // Assert
        Assert.Empty(descriptor.NullableRouteParameters);
    }

    [Fact]
    public void Build_PageableQuery_PagingParamsBoundAsQueryNotBody()
    {
        // Arrange — PagingParams is a complex property under [AsParameters]; a GET query allows no body, so it must
        // be query-bound, never treated as a body payload (regression guard for the pageable-query path).
        static PageableSampleQuery Handler([AsParameters] PageableSampleQuery q) => q;

        // Act
        var descriptor = CqrsEndpointDescriptorBuilder.Build(
            Handler,
            "GET",
            "articles",
            isQuery: true,
            typeof(PageableSampleQuery),
            typeof(PagedList<SampleDto>),
            errorType: null);

        // Assert
        Assert.Equal(ResponseShape.PagedList, descriptor.ResponseShape);
        Assert.Null(descriptor.PayloadType);
        Assert.DoesNotContain(descriptor.Parameters, p => p.Source == ParameterSource.Body);
        var pagingParams = Single(descriptor.Parameters, p => p.Name == "PagingParams");
        Assert.Equal(ParameterSource.Query, pagingParams.Source);
    }

    private static T Single<T>(IEnumerable<T> source, Func<T, bool>? predicate = null)
    {
        var filtered = predicate is null ? source.ToList() : source.Where(predicate).ToList();
        Assert.True(filtered.Count == 1, $"Expected exactly one element, found {filtered.Count}.");
        return filtered[0];
    }
}
