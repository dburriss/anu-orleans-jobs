namespace Anu.Jobs.Tests.Acceptance.Infrastructure;

/// <summary>
/// Defines a test collection that shares an Orleans cluster across multiple test classes.
/// This ensures tests run sequentially within the collection for proper isolation.
/// </summary>
[CollectionDefinition(Name)]
public sealed class OrleansAcceptanceCollection : ICollectionFixture<OrleansClusterFixture>
{
    public const string Name = "Orleans Acceptance Collection";
}
