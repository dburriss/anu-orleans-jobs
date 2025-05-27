using Anu.Jobs.Grains;
using Microsoft.Extensions.DependencyInjection;
using Orleans.TestingHost;

namespace Anu.Jobs.Tests.Acceptance.Infrastructure;

/// <summary>
/// Base class for acceptance tests providing common Orleans cluster access.
/// </summary>
[Collection(OrleansAcceptanceCollection.Name)]
public abstract class AcceptanceTestBase : IAsyncLifetime
{
    protected TestCluster Cluster { get; }
    protected IGrainFactory GrainFactory => Cluster.GrainFactory;

    protected AcceptanceTestBase(OrleansClusterFixture fixture)
    {
        Cluster = fixture.Cluster;
    }

    /// <summary>
    /// Override this method to perform test-specific setup.
    /// </summary>
    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Override this method to perform test-specific cleanup.
    /// </summary>
    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Creates a unique job grain for testing.
    /// </summary>
    protected async Task<IJobGrain> CreateTestJobAsync<TJob>(string? name = null)
        where TJob : class, IJob
    {
        var jobName = name ?? $"{typeof(TJob).Name}_{Guid.NewGuid():N}";
        
        // Create JobBuilder directly instead of getting from DI
        var jobBuilder = new JobBuilder(GrainFactory);
        return await jobBuilder.StartJobAsync<TJob>(jobName);
    }
}
