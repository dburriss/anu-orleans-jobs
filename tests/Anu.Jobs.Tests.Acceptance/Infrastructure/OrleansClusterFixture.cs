using Microsoft.Extensions.DependencyInjection;
using Orleans.TestingHost;

namespace Anu.Jobs.Tests.Acceptance.Infrastructure;

/// <summary>
/// Provides a shared Orleans test cluster for acceptance tests.
/// </summary>
public sealed class OrleansClusterFixture : IDisposable
{
    public TestCluster Cluster { get; }

    public OrleansClusterFixture()
    {
        var builder = new TestClusterBuilder()
            .AddSiloBuilderConfigurator<TestSiloConfigurator>();
        
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose()
    {
        Cluster?.StopAllSilos();
    }
}

/// <summary>
/// Configures the test silo with job services.
/// </summary>
file sealed class TestSiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.AddJobs(); // Register job framework
        siloBuilder.ConfigureServices(services =>
        {
            // Add any test-specific services here
            services.AddTransient<IJobRunner, JobRunner>();
        });
    }
}
