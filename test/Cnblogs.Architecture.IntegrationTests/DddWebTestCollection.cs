namespace Cnblogs.Architecture.IntegrationTests;

[CollectionDefinition(Name)]
public class DddWebTestCollection : ICollectionFixture<DddWebTestFactory>
{
    public const string Name = nameof(DddWebTestCollection);
}
