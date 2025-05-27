namespace Anu.Jobs.Tests.Acceptance.TestJobs;

/// <summary>
/// A simple test job that can be configured to succeed, fail, or delay.
/// </summary>
public class SampleTestJob : IJob
{
    public static bool ShouldFail { get; set; } = false;
    public static TimeSpan ExecutionDelay { get; set; } = TimeSpan.Zero;
    public static List<string> ExecutionLog { get; } = new();

    public async Task Execute(JobContext context)
    {
        ExecutionLog.Add($"Execute: {context.JobName} at {DateTime.UtcNow:HH:mm:ss.fff}");
        
        if (ExecutionDelay > TimeSpan.Zero)
        {
            await Task.Delay(ExecutionDelay, context.CancellationToken);
        }

        if (ShouldFail)
        {
            throw new InvalidOperationException("Test job configured to fail");
        }
    }

    public Task Compensate(JobContext context)
    {
        ExecutionLog.Add($"Compensate: {context.JobName} at {DateTime.UtcNow:HH:mm:ss.fff}");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Resets the test job state between tests.
    /// </summary>
    public static void Reset()
    {
        ShouldFail = false;
        ExecutionDelay = TimeSpan.Zero;
        ExecutionLog.Clear();
    }
}
