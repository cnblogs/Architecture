using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Domain.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.ServiceAgent.Design;

namespace Cnblogs.Architecture.UnitTests.Cqrs;

public class ClrTypeRefTests
{
    public record SampleDto(int Id, string Title);

    public class TestError : Enumeration
    {
        public static readonly TestError None = new(0, nameof(None));

        public TestError(int id, string name)
            : base(id, name)
        {
        }
    }

    [Fact]
    public void FromType_PrimitiveInt_HasSystemNamespaceAndInt32Name()
    {
        // Act
        var refType = ClrTypeRef.FromType(typeof(int));

        // Assert
        Assert.Equal("System", refType.Namespace);
        Assert.Equal("Int32", refType.Name);
        Assert.False(refType.IsNullable);
        Assert.False(refType.IsArray);
        Assert.Empty(refType.GenericArguments);
    }

    [Fact]
    public void FromType_String_HasSystemNamespaceAndStringName()
    {
        // Act
        var refType = ClrTypeRef.FromType(typeof(string));

        // Assert
        Assert.Equal("System", refType.Namespace);
        Assert.Equal("String", refType.Name);
        Assert.False(refType.IsNullable);
        Assert.False(refType.IsArray);
        Assert.Empty(refType.GenericArguments);
    }

    [Fact]
    public void FromType_NullableOfInt_MarksNullableAndKeepsInnerType()
    {
        // Act
        var refType = ClrTypeRef.FromType(typeof(int?));

        // Assert
        Assert.True(refType.IsNullable);
        Assert.False(refType.IsArray);
        Assert.Equal("System", refType.Namespace);
        Assert.Equal("Int32", refType.Name);
    }

    [Fact]
    public void FromType_SingleDimensionalIntArray_MarksArrayRankOneWithElement()
    {
        // Act
        var refType = ClrTypeRef.FromType(typeof(int[]));

        // Assert
        Assert.True(refType.IsArray);
        Assert.Equal(1, refType.ArrayRank);
        Assert.Single(refType.GenericArguments);
        Assert.Equal("Int32", refType.GenericArguments[0].Name);
    }

    [Fact]
    public void FromType_TwoDimensionalIntArray_PreservesArrayRankTwo()
    {
        // Act
        var refType = ClrTypeRef.FromType(typeof(int[,]));

        // Assert
        Assert.True(refType.IsArray);
        Assert.Equal(2, refType.ArrayRank);
        Assert.Single(refType.GenericArguments);
        Assert.Equal("Int32", refType.GenericArguments[0].Name);
    }

    [Fact]
    public void FromType_ListOfString_HasGenericNamespaceAndOneGenericArgument()
    {
        // Act
        var refType = ClrTypeRef.FromType(typeof(List<string>));

        // Assert
        Assert.Equal("System.Collections.Generic", refType.Namespace);
        Assert.Equal("List", refType.Name);
        Assert.Single(refType.GenericArguments);
        Assert.Equal("String", refType.GenericArguments[0].Name);
    }

    [Fact]
    public void FromType_PagedListOfSampleDto_HasInfrastructureNamespaceAndOneGenericArgument()
    {
        // Act
        var refType = ClrTypeRef.FromType(typeof(PagedList<SampleDto>));

        // Assert — SampleDto is nested in this test class, so its simple name carries the declaring-type prefix.
        Assert.Equal("Cnblogs.Architecture.Ddd.Infrastructure.Abstractions", refType.Namespace);
        Assert.Equal("PagedList", refType.Name);
        Assert.Single(refType.GenericArguments);
        Assert.EndsWith("SampleDto", refType.GenericArguments[0].Name);
    }

    [Fact]
    public void FromType_CommandResponseWithTwoGenericArgs_ExposesBothArguments()
    {
        // Arrange — construct the closed generic via reflection so we don't depend on a directly referenceable open type.
        var closedType = typeof(CommandResponse<,>).MakeGenericType(typeof(string), typeof(TestError));

        // Act
        var refType = ClrTypeRef.FromType(closedType);

        // Assert
        Assert.Equal("CommandResponse", refType.Name);
        Assert.Equal(2, refType.GenericArguments.Length);
        Assert.Equal("String", refType.GenericArguments[0].Name);
        // TestError is nested in this test class, so its simple name carries the declaring-type prefix.
        Assert.EndsWith("TestError", refType.GenericArguments[1].Name);
    }
}
