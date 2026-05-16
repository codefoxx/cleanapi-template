namespace Company.Template.Application.Tests.TestSupport;

[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<TestDatabaseServer>
{
    public const string Name = "Database";
}
