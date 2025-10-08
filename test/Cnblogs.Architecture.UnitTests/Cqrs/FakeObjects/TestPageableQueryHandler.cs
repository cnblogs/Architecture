using Cnblogs.Architecture.Ddd.Cqrs.EntityFramework;
using Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;
using Microsoft.EntityFrameworkCore;

namespace Cnblogs.Architecture.UnitTests.Cqrs.FakeObjects;

public class TestPageableQueryHandler(DbContext context)
    : EfPageableQueryHandler<TestPageableQuery, FakePost, FakePostDto>
{
    /// <inheritdoc />
    protected override IQueryable<FakePost> DefaultOrderBy(TestPageableQuery query, IQueryable<FakePost> queryable)
    {
        return queryable.OrderByDescending(x => x.DateAdded).ThenByDescending(x => x.DateUpdated);
    }

    /// <inheritdoc />
    protected override IQueryable<FakePost> DefaultReverseOrderBy(TestPageableQuery query, IQueryable<FakePost> queryable)
    {
        return queryable.OrderBy(x => x.DateAdded).ThenBy(x => x.DateUpdated);
    }

    /// <inheritdoc />
    protected override IQueryable<FakePost> Filter(TestPageableQuery query)
    {
        var queryable = context.Set<FakePost>().AsNoTracking();
        if (query.Deleted.HasValue)
        {
            queryable = queryable.Where(x => x.Deleted == query.Deleted.Value);
        }

        return queryable;
    }
}
