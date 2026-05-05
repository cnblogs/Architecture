using System.Collections.ObjectModel;
using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;
using MediatR;
using NSubstitute;

namespace Cnblogs.Architecture.UnitTests.Cqrs.Behaviors;

public class EnricherBehaviorTests
{
    private static EnricherBehavior<TRequest, TResponse> CreateBehavior<TRequest, TResponse>(
        IServiceProvider sp)
        where TRequest : IEnrichableRequest, IRequest<TResponse>
        where TResponse : class
    {
        return new EnricherBehavior<TRequest, TResponse>(sp, new EnricherMappingCache());
    }

    private static (TrackingEnricher Enricher, IServiceProvider Sp) CreateSpWithEnricher()
    {
        var enricher = new TrackingEnricher();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(IEnumerable<IEnricher<FakePostDto>>))
            .Returns(new[] { enricher as IEnricher<FakePostDto> });
        return (enricher, sp);
    }

    private static IServiceProvider CreateSpWithoutEnricher()
    {
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(IEnumerable<IEnricher<FakePostDto>>))
            .Returns(Array.Empty<IEnricher<FakePostDto>>());
        return sp;
    }

    [Fact]
    public async Task EarlyReturn_NullResponse_NoEnrichmentAsync()
    {
        // Arrange
        var sp = CreateSpWithoutEnricher();
        var behavior = CreateBehavior<FakeEnrichableRequest<FakePostDto>, FakePostDto>(sp);

        // Act
        var result = await behavior.Handle(
            new FakeEnrichableRequest<FakePostDto>(),
            _ => Task.FromResult<FakePostDto?>(null),
            CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EarlyReturn_SkipEnrichRequested_NoEnrichmentAsync()
    {
        // Arrange
        var (enricher, sp) = CreateSpWithEnricher();
        var dto = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var behavior = CreateBehavior<FakeEnrichableRequest<FakePostDto>, FakePostDto>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<FakePostDto> { SkipEnrich = true },
            _ => Task.FromResult<FakePostDto?>(dto),
            CancellationToken.None);

        // Assert
        Assert.Empty(enricher.EnrichedItems);
    }

    [Fact]
    public async Task EarlyReturn_StringResponse_NoEnrichmentAsync()
    {
        // Arrange
        var sp = CreateSpWithoutEnricher();
        var behavior = CreateBehavior<FakeEnrichableRequest<string>, string>(sp);

        // Act
        var result = await behavior.Handle(
            new FakeEnrichableRequest<string>(),
            _ => Task.FromResult<string?>("hello"),
            CancellationToken.None);

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task EarlyReturn_ElementTypeIsValueType_NoEnrichmentAsync()
    {
        // Arrange
        var sp = CreateSpWithoutEnricher();
        var behavior = CreateBehavior<FakeEnrichableRequest<List<int>>, List<int>>(sp);

        // Act
        var result = await behavior.Handle(
            new FakeEnrichableRequest<List<int>>(),
            _ => Task.FromResult<List<int>?>([1, 2, 3]),
            CancellationToken.None);

        // Assert
        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public async Task EarlyReturn_NoEnricherRegistered_NoErrorAsync()
    {
        // Arrange
        var sp = CreateSpWithoutEnricher();
        var dto = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var behavior = CreateBehavior<FakeEnrichableRequest<FakePostDto>, FakePostDto>(sp);

        // Act
        var result = await behavior.Handle(
            new FakeEnrichableRequest<FakePostDto>(),
            _ => Task.FromResult<FakePostDto?>(dto),
            CancellationToken.None);

        // Assert
        Assert.Same(dto, result);
    }

    [Fact]
    public async Task SingleObject_ResponseIsSingleItem_EnrichedAsync()
    {
        // Arrange
        var dto = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var (enricher, sp) = CreateSpWithEnricher();
        var behavior = CreateBehavior<FakeEnrichableRequest<FakePostDto>, FakePostDto>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<FakePostDto>(),
            _ => Task.FromResult<FakePostDto?>(dto),
            CancellationToken.None);

        // Assert
        Assert.Single(enricher.EnrichedItems);
        Assert.Same(dto, enricher.EnrichedItems[0]);
    }

    [Fact]
    public async Task Enumerable_ListResponse_CallsBulkEnrichAsync()
    {
        // Arrange
        var dto1 = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var dto2 = new FakePostDto(2, DateTimeOffset.Now, DateTimeOffset.Now);
        var (enricher, sp) = CreateSpWithEnricher();
        var behavior = CreateBehavior<FakeEnrichableRequest<List<FakePostDto>>, List<FakePostDto>>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<List<FakePostDto>>(),
            _ => Task.FromResult<List<FakePostDto>?>([dto1, dto2]),
            CancellationToken.None);

        // Assert
        Assert.Equal(2, enricher.EnrichedItems.Count);
        Assert.Same(dto1, enricher.EnrichedItems[0]);
        Assert.Same(dto2, enricher.EnrichedItems[1]);
    }

    [Fact]
    public async Task PagedList_ResponseIsPagedList_CallsBulkEnrichAsync()
    {
        // Arrange
        var dto1 = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var dto2 = new FakePostDto(2, DateTimeOffset.Now, DateTimeOffset.Now);
        var pagedList = new PagedList<FakePostDto>([dto1, dto2], 1, 10, 2);
        var (enricher, sp) = CreateSpWithEnricher();
        var behavior =
            CreateBehavior<FakeEnrichableRequest<PagedList<FakePostDto>>, PagedList<FakePostDto>>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<PagedList<FakePostDto>>(),
            _ => Task.FromResult<PagedList<FakePostDto>?>(pagedList),
            CancellationToken.None);

        // Assert
        Assert.Equal(2, enricher.EnrichedItems.Count);
        Assert.Same(dto1, enricher.EnrichedItems[0]);
        Assert.Same(dto2, enricher.EnrichedItems[1]);
    }

    [Fact]
    public async Task PagedList_ResponseIsEmpty_NoEnrichmentAsync()
    {
        // Arrange
        var pagedList = new PagedList<FakePostDto>();
        var (enricher, sp) = CreateSpWithEnricher();
        var behavior =
            CreateBehavior<FakeEnrichableRequest<PagedList<FakePostDto>>, PagedList<FakePostDto>>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<PagedList<FakePostDto>>(),
            _ => Task.FromResult<PagedList<FakePostDto>?>(pagedList),
            CancellationToken.None);

        // Assert
        Assert.Empty(enricher.EnrichedItems);
    }

    [Fact]
    public async Task Dictionary_ResponseIsDictionary_EnrichesValuesAsync()
    {
        // Arrange
        var dto1 = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var dto2 = new FakePostDto(2, DateTimeOffset.Now, DateTimeOffset.Now);
        var dict = new Dictionary<int, FakePostDto> { { 1, dto1 }, { 2, dto2 } };
        var (enricher, sp) = CreateSpWithEnricher();
        var behavior =
            CreateBehavior<FakeEnrichableRequest<Dictionary<int, FakePostDto>>, Dictionary<int, FakePostDto>>(
                sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<Dictionary<int, FakePostDto>>(),
            _ => Task.FromResult<Dictionary<int, FakePostDto>?>(dict),
            CancellationToken.None);

        // Assert
        Assert.Equal(2, enricher.EnrichedItems.Count);
    }

    [Fact]
    public async Task Dictionary_ResponseIsReadOnlyDictionary_EnrichesValuesAsync()
    {
        // Arrange
        var dto1 = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var dto2 = new FakePostDto(2, DateTimeOffset.Now, DateTimeOffset.Now);
        var dict = new ReadOnlyDictionary<int, FakePostDto>(
            new Dictionary<int, FakePostDto> { { 1, dto1 }, { 2, dto2 } });
        var (enricher, sp) = CreateSpWithEnricher();
        var behavior = CreateBehavior<FakeEnrichableRequest<ReadOnlyDictionary<int, FakePostDto>>,
            ReadOnlyDictionary<int, FakePostDto>>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<ReadOnlyDictionary<int, FakePostDto>>(),
            _ => Task.FromResult<ReadOnlyDictionary<int, FakePostDto>?>(dict),
            CancellationToken.None);

        // Assert
        Assert.Equal(2, enricher.EnrichedItems.Count);
    }

    [Fact]
    public async Task ObjectResponse_WrapsSingleItem_EnrichedAsync()
    {
        // Arrange
        var dto = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var response = new FakeObjectResponse<FakePostDto>(dto);
        var (enricher, sp) = CreateSpWithEnricher();
        var behavior = CreateBehavior<FakeEnrichableRequest<FakeObjectResponse<FakePostDto>>,
            FakeObjectResponse<FakePostDto>>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<FakeObjectResponse<FakePostDto>>(),
            _ => Task.FromResult<FakeObjectResponse<FakePostDto>?>(response),
            CancellationToken.None);

        // Assert
        Assert.Single(enricher.EnrichedItems);
        Assert.Same(dto, enricher.EnrichedItems[0]);
    }

    [Fact]
    public async Task ObjectResponse_WrapsNullResult_NoEnrichmentAsync()
    {
        // Arrange
        var response = new FakeObjectResponse<FakePostDto>(null);
        var (enricher, sp) = CreateSpWithEnricher();
        var behavior = CreateBehavior<FakeEnrichableRequest<FakeObjectResponse<FakePostDto>>,
            FakeObjectResponse<FakePostDto>>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<FakeObjectResponse<FakePostDto>>(),
            _ => Task.FromResult<FakeObjectResponse<FakePostDto>?>(response),
            CancellationToken.None);

        // Assert
        Assert.Empty(enricher.EnrichedItems);
    }

    [Fact]
    public async Task ObjectResponse_WrapsPagedList_UnwrapsBothAsync()
    {
        // Arrange
        var dto1 = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var dto2 = new FakePostDto(2, DateTimeOffset.Now, DateTimeOffset.Now);
        var pagedList = new PagedList<FakePostDto>([dto1, dto2], 1, 10, 2);
        var response = new FakeObjectResponse<PagedList<FakePostDto>>(pagedList);
        var (enricher, sp) = CreateSpWithEnricher();
        var behavior = CreateBehavior<FakeEnrichableRequest<FakeObjectResponse<PagedList<FakePostDto>>>,
            FakeObjectResponse<PagedList<FakePostDto>>>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<FakeObjectResponse<PagedList<FakePostDto>>>(),
            _ => Task.FromResult<FakeObjectResponse<PagedList<FakePostDto>>?>(response),
            CancellationToken.None);

        // Assert
        Assert.Equal(2, enricher.EnrichedItems.Count);
    }

    [Fact]
    public async Task NestedContainer_PagedListDictionary_FlattensAndEnrichesAsync()
    {
        // Arrange
        var dto1 = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var dto2 = new FakePostDto(2, DateTimeOffset.Now, DateTimeOffset.Now);
        var dict = new Dictionary<int, FakePostDto> { { 1, dto1 }, { 2, dto2 } };
        var pagedList = new PagedList<Dictionary<int, FakePostDto>>([dict], 1, 10, 1);
        var (enricher, sp) = CreateSpWithEnricher();
        var behavior = CreateBehavior<
            FakeEnrichableRequest<PagedList<Dictionary<int, FakePostDto>>>,
            PagedList<Dictionary<int, FakePostDto>>>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<PagedList<Dictionary<int, FakePostDto>>>(),
            _ => Task.FromResult<PagedList<Dictionary<int, FakePostDto>>?>(pagedList),
            CancellationToken.None);

        // Assert
        Assert.Equal(2, enricher.EnrichedItems.Count);
        Assert.Contains(dto1, enricher.EnrichedItems);
        Assert.Contains(dto2, enricher.EnrichedItems);
    }

    [Fact]
    public async Task NestedContainer_DictionaryList_FlattensAndEnrichesAsync()
    {
        // Arrange
        var dto1 = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var dto2 = new FakePostDto(2, DateTimeOffset.Now, DateTimeOffset.Now);
        var list = new List<FakePostDto> { dto1, dto2 };
        var dict = new Dictionary<int, List<FakePostDto>> { { 1, list } };
        var (enricher, sp) = CreateSpWithEnricher();
        var behavior = CreateBehavior<
            FakeEnrichableRequest<Dictionary<int, List<FakePostDto>>>,
            Dictionary<int, List<FakePostDto>>>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<Dictionary<int, List<FakePostDto>>>(),
            _ => Task.FromResult<Dictionary<int, List<FakePostDto>>?>(dict),
            CancellationToken.None);

        // Assert
        Assert.Equal(2, enricher.EnrichedItems.Count);
    }

    [Fact]
    public async Task MultipleEnrichers_MultipleRegistered_BothCalledAsync()
    {
        // Arrange
        var dto = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var enricher1 = new TrackingEnricher();
        var enricher2 = new TrackingEnricher();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(IEnumerable<IEnricher<FakePostDto>>))
            .Returns(new IEnricher<FakePostDto>[] { enricher1, enricher2 });
        var behavior = CreateBehavior<FakeEnrichableRequest<FakePostDto>, FakePostDto>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<FakePostDto>(),
            _ => Task.FromResult<FakePostDto?>(dto),
            CancellationToken.None);

        // Assert
        Assert.Single(enricher1.EnrichedItems);
        Assert.Single(enricher2.EnrichedItems);
    }

    [Fact]
    public async Task MultipleEnrichers_AllAllowParallel_BothCalledAsync()
    {
        // Arrange
        var dto = new FakePostDto(1, DateTimeOffset.Now, DateTimeOffset.Now);
        var enricher1 = new TrackingEnricher { AllowParallel = true };
        var enricher2 = new TrackingEnricher { AllowParallel = true };
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(IEnumerable<IEnricher<FakePostDto>>))
            .Returns(new IEnricher<FakePostDto>[] { enricher1, enricher2 });
        var behavior = CreateBehavior<FakeEnrichableRequest<FakePostDto>, FakePostDto>(sp);

        // Act
        await behavior.Handle(
            new FakeEnrichableRequest<FakePostDto>(),
            _ => Task.FromResult<FakePostDto?>(dto),
            CancellationToken.None);

        // Assert
        Assert.Single(enricher1.EnrichedItems);
        Assert.Single(enricher2.EnrichedItems);
    }
}

public record FakeObjectResponse<T>(T? Result) : IObjectResponse
{
    public object? GetResult() => Result;
}
