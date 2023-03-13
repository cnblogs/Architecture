using Microsoft.EntityFrameworkCore;

namespace Cnblogs.Architecture.UnitTests.Infrastructure.FakeObjects;

public class FakeDbContext : DbContext
{
    public FakeDbContext(DbContextOptions options)
        : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FakeBlog>().HasKey(x => x.Id);
        modelBuilder.Entity<FakeBlog>().Property(x => x.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<FakeBlog>().HasMany(x => x.Posts).WithOne(x => x.Blog).HasForeignKey(x => x.BlogId);

        modelBuilder.Entity<FakePost>().HasKey(x => x.Id);
        modelBuilder.Entity<FakePost>().Property(x => x.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<FakePost>().HasMany(x => x.Tags).WithOne().HasForeignKey(x => x.PostId);

        modelBuilder.Entity<FakeTag>().HasKey(x => x.Id);
        modelBuilder.Entity<FakeTag>().Property(x => x.Id).ValueGeneratedOnAdd();
    }
}
