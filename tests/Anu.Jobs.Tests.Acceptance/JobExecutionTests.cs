using Anu.Jobs.Tests.Acceptance.Infrastructure;
using Anu.Jobs.Tests.Acceptance.TestJobs;

namespace Anu.Jobs.Tests.Acceptance;

/// <summary>
/// Acceptance tests for job execution scenarios.
/// </summary>
public class JobExecutionTests : AcceptanceTestBase
{
    public JobExecutionTests(OrleansClusterFixture fixture) : base(fixture) { }

    public override ValueTask InitializeAsync()
    {
        // Reset test job state before each test
        SampleTestJob.Reset();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task WhenJobExecutes_ThenCompletesSuccessfully()
    {
        // Arrange
        var jobGrain = await CreateTestJobAsync<SampleTestJob>();

        // Act
        await jobGrain.TriggerExecution();

        // Assert
        var state = await jobGrain.GetState();
        Assert.Equal(JobStage.Completed, state.CurrentRun.Stage);
    }

    [Fact]
    public async Task WhenJobFailsWithMaxRetriesIsZero_ThenWillFail()
    {
        // Arrange
        SampleTestJob.ShouldFail = true;
        var jobGrain = await CreateTestJobAsync<SampleTestJob>();

        // Act
        await jobGrain.TriggerExecution();

        // Assert
        var state = await jobGrain.GetState();
        await Task.Delay(10);
        Assert.Equal(JobStage.Failed, state.CurrentRun.Stage);
    }

    [Fact(Explicit = true)]
    public async Task WhenJobIsScheduled_ThenExecutesAtScheduledTime()
    {
        // Arrange
        var jobGrain = await CreateTestJobAsync<SampleTestJob>();
        var scheduledTime = DateTimeOffset.UtcNow.AddSeconds(61); // Orleans minimum is 1 minute

        // Act
        await jobGrain.ScheduleExecution(scheduledTime);

        // Wait a bit longer than scheduled time to ensure execution completes
        await Task.Delay(TimeSpan.FromSeconds(65));

        // Assert
        var state = await jobGrain.GetState();
        Assert.Equal(JobStage.Completed, state.CurrentRun.Stage);
        Assert.Contains("Execute:", SampleTestJob.ExecutionLog);
    }
}
