using System.Security.Claims;
using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.Mcp;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.AgentFramework;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using ModelContextProtocol.Server;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Mcp;

#pragma warning disable SA1649
public record McpGetThingQuery(int Id) : IQuery<string>;

public record McpCreateThingCommand(string Title, bool ValidateOnly = false) : ICommand<AgentTestError>;

public record McpListThingsQuery(PagingParams? PagingParams, string? OrderByString) : IPageableQuery<string>;

/// <summary>Query an article with documented parameters.</summary>
/// <param name="Id">The id of the article.</param>
/// <param name="IncludeBody">Whether to include the body.</param>
public record McpDocumentedQuery(int Id, bool IncludeBody) : IQuery<string>;
#pragma warning restore SA1649

public class MapMcpToolTests
{
    [Fact]
    public void MapMcpTool_DelegateQuery_RegistersNameDescriptionAndReadOnlyAnnotation()
    {
        // Arrange
        var (app, tools) = BuildApp();

        // Act
        app.MapMcpTool("get_thing", (int id) => new McpGetThingQuery(id), "Get a thing by id.");

        // Assert
        var tool = Assert.Single(tools.Tools);
        Assert.Equal("get_thing", tool.ProtocolTool.Name);
        Assert.Equal("Get a thing by id.", tool.ProtocolTool.Description);
        Assert.True(tool.ProtocolTool.Annotations?.ReadOnlyHint);
        Assert.Contains(SchemaProperties(tool), n => n == "id");
    }

    [Fact]
    public void MapMcpTool_DelegateCommand_HasNoReadOnlyAnnotation()
    {
        // Arrange
        var (app, tools) = BuildApp();

        // Act
        app.MapMcpTool("create_thing", (string title) => new McpCreateThingCommand(title), "Create a thing.");

        // Assert
        var tool = Assert.Single(tools.Tools);
        Assert.False(tool.ProtocolTool.Annotations?.ReadOnlyHint ?? false);
    }

    [Fact]
    public void MapMcpTool_DelegateWithClaimsPrincipal_HidesInjectedParameterFromSchema()
    {
        // Arrange
        var (app, tools) = BuildApp();

        // Act
        app.MapMcpTool(
            "create_for_user",
            (ClaimsPrincipal user, string title) => new McpCreateThingCommand(title),
            "Create a thing for the current user.");

        // Assert
        var tool = Assert.Single(tools.Tools);
        var properties = SchemaProperties(tool);
        Assert.Contains(properties, n => n == "title");
        Assert.DoesNotContain(properties, n => n == "user");
    }

    [Fact]
    public void MapMcpTool_RecordCommand_FlattensSchemaAndHidesInfrastructureParameters()
    {
        // Arrange
        var (app, tools) = BuildApp();

        // Act
        app.MapMcpTool<McpCreateThingCommand>("create_thing", "Create a thing.");

        // Assert
        var tool = Assert.Single(tools.Tools);
        var properties = SchemaProperties(tool);
        Assert.Contains(properties, n => n == "title");
        Assert.DoesNotContain(properties, n => n == "validateOnly");
    }

    [Fact]
    public void MapMcpTool_RecordPageableQuery_FlattensPagingParameters()
    {
        // Arrange
        var (app, tools) = BuildApp();

        // Act
        app.MapMcpTool<McpListThingsQuery>("list_things", "List things.");

        // Assert
        var tool = Assert.Single(tools.Tools);
        var properties = SchemaProperties(tool);
        Assert.Contains(properties, n => n == "pageIndex");
        Assert.Contains(properties, n => n == "pageSize");
        Assert.DoesNotContain(properties, n => n == "pagingParams");
        Assert.DoesNotContain(properties, n => n == "orderByString");
    }

    [Fact]
    public void MapMcpTool_Configure_CanOverrideToolMetadata()
    {
        // Arrange
        var (app, tools) = BuildApp();

        // Act
        app.MapMcpTool<McpGetThingQuery>(
            "get_thing",
            "Get a thing.",
            o => o.Title = "Get thing");

        // Assert
        var tool = Assert.Single(tools.Tools);
        Assert.Equal("Get thing", tool.ProtocolTool.Title);
    }

    [Fact]
    public void MapMcpToolGroup_PrefixesToolNames()
    {
        // Arrange
        var (app, tools) = BuildApp();

        // Act
        var group = app.MapMcpToolGroup("things");
        group.MapMcpTool("get", (int id) => new McpGetThingQuery(id), "Get a thing.");
        group.MapMcpTool<McpCreateThingCommand>("create", "Create a thing.");

        // Assert
        Assert.Equal(
            ["things_create", "things_get"],
            tools.Tools.Select(t => t.ProtocolTool.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void MapMcpTool_LambdaParameterDescription_FlowsIntoSchema()
    {
        // Arrange — lambda parameter attributes (C# 10+, the same mechanism minimal APIs use) describe tool arguments.
        var (app, tools) = BuildApp();

        // Act
        app.MapMcpTool(
            "get_thing",
            ([System.ComponentModel.Description("The id of the thing.")] int id) => new McpGetThingQuery(id),
            "Get a thing.");

        // Assert
        var tool = Assert.Single(tools.Tools);
        var description = tool.ProtocolTool.InputSchema.GetProperty("properties").GetProperty("id")
            .GetProperty("description").GetString();
        Assert.Equal("The id of the thing.", description);
    }

    [Fact]
    public void MapMcpTool_Record_UsesXmlDocParameterDescriptionsAndSummaryFallback()
    {
        // Arrange
        var (app, tools) = BuildApp();

        // Act — no description provided; the XML doc <summary> becomes the tool description.
        app.MapMcpTool<McpDocumentedQuery>("get_documented");

        // Assert
        var tool = Assert.Single(tools.Tools);
        Assert.Equal("Query an article with documented parameters.", tool.ProtocolTool.Description);
        var properties = tool.ProtocolTool.InputSchema.GetProperty("properties");
        Assert.Equal("The id of the article.", properties.GetProperty("id").GetProperty("description").GetString());
        Assert.Equal("Whether to include the body.", properties.GetProperty("includeBody").GetProperty("description").GetString());
    }

    [Fact]
    public void MapMcpTool_DuplicateName_Throws()
    {
        // Arrange
        var (app, tools) = BuildApp();
        app.MapMcpTool("get_thing", (int id) => new McpGetThingQuery(id), "Get a thing.");

        // Act
        var act = () => app.MapMcpTool("get_thing", (int id) => new McpGetThingQuery(id), "Get a thing.");

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    private static (WebApplication App, CqrsMcpToolCollection Tools) BuildApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<CqrsMcpToolCollection>();
        var app = builder.Build();
        return (app, app.Services.GetRequiredService<CqrsMcpToolCollection>());
    }

    private static IEnumerable<string> SchemaProperties(McpServerTool tool)
    {
        var schema = tool.ProtocolTool.InputSchema;
        return schema.TryGetProperty("properties", out var properties) is false
            ? []
            : properties.EnumerateObject().Select(p => p.Name).ToArray();
    }
}
