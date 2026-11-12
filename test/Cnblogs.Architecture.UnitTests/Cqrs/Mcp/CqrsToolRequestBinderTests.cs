using Cnblogs.Architecture.Ddd.Cqrs.Mcp;

using Microsoft.Extensions.AI;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Mcp;

public class CqrsToolRequestBinderTests
{
    [Fact]
    public void Bind_MapsArgumentsToRecordProperties()
    {
        // Arrange
        var arguments = new AIFunctionArguments { ["title"] = "hello" };

        // Act
        var request = CqrsToolRequestBinder<McpCreateThingCommand>.Bind(arguments);

        // Assert
        Assert.Equal("hello", request.Title);
        Assert.False(request.ValidateOnly);
    }

    [Fact]
    public void Bind_IgnoresInfrastructureArgumentsFromTheModel()
    {
        // Arrange — the model tries to set ValidateOnly/ordering; both are hidden and server-bound.
        var arguments = new AIFunctionArguments { ["title"] = "hello", ["validateOnly"] = true };

        // Act
        var request = CqrsToolRequestBinder<McpCreateThingCommand>.Bind(arguments);

        // Assert
        Assert.False(request.ValidateOnly);
    }

    [Fact]
    public void Bind_FlattensAndClampsPagingParameters()
    {
        // Arrange
        var arguments = new AIFunctionArguments { ["pageIndex"] = 0, ["pageSize"] = 999 };

        // Act
        var request = CqrsToolRequestBinder<McpListThingsQuery>.Bind(arguments);

        // Assert
        Assert.NotNull(request.PagingParams);
        Assert.Equal(1, request.PagingParams.PageIndex);
        Assert.Equal(CqrsToolRequestBinder<McpListThingsQuery>.MaxPageSize, request.PagingParams.PageSize);
    }

    [Fact]
    public void Bind_DefaultsPagingWhenModelOmitsIt()
    {
        // Arrange
        var arguments = new AIFunctionArguments();

        // Act
        var request = CqrsToolRequestBinder<McpListThingsQuery>.Bind(arguments);

        // Assert
        Assert.NotNull(request.PagingParams);
        Assert.Equal(1, request.PagingParams.PageIndex);
        Assert.Equal(CqrsToolRequestBinder<McpListThingsQuery>.MaxPageSize, request.PagingParams.PageSize);
    }
}
