namespace Cnblogs.Architecture.IntegrationTests;

[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFactory>
{
    public const string Name = nameof(IntegrationTestCollection);
}
