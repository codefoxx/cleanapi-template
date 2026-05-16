namespace Company.Template.Infrastructure.Tests.TestSupport;

[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<TestDatabaseServer>
{
    public const string Name = "Database";
}
