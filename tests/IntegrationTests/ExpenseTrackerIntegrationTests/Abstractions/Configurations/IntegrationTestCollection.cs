namespace ExpenseTracker.IntegrationTests.Abstractions.Configurations;

// Marker class — the ONLY place ICollectionFixture<T> is allowed.
[CollectionDefinition("Integration tests")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
}