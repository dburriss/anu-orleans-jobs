namespace Anu.Jobs;

/// <summary>
/// Represents the persistent state of a job execution within an Orleans grain.
/// This state can be persisted and recovered if the grain is deactivated or fails.
/// </summary>
[GenerateSerializer]
public class JobState
{
    /// <summary>
    /// Gets or sets the definition of the job being executed.
    /// </summary>
    [Id(0)]
    public required JobDefinition JobDefinition { get; set; }

    /// <summary>
    /// Gets or sets information about the current run of this job.
    /// </summary>
    [Id(1)]
    public JobRunInfo CurrentRun { get; internal set; } = new JobRunInfo();

    /// <summary>
    /// Gets or sets information about the previous run of this job (for recurring jobs).
    /// </summary>
    [Id(2)]
    public JobRunInfo? PreviousRun { get; internal set; }

    /// <summary>
    /// Gets or sets the time of next scheduled execution.
    /// </summary>
    public DateTimeOffset ScheduledTime { get; internal set; }

    /// <summary>
    /// Gets or sets the triggers that can initiate this job.
    /// </summary>
    [Id(3)]
    public JobTriggers Triggers { get; set; } = new JobTriggers();

    /// <summary>
    /// Creates a JobContext from this state for passing to the job implementation.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the job execution.</param>
    /// <returns>A new JobContext instance.</returns>
    public JobContext CreateJobContext(CancellationToken cancellationToken = default)
    {
        return new JobContext
        {
            JobName = JobDefinition.JobName,
            RunId = CurrentRun.RunId.ToString(),
            StartTime = CurrentRun.StartedAt ?? DateTimeOffset.UtcNow,
            CancellationToken = cancellationToken,
        };
    }

    /// <summary>
    /// Marks the job as started.
    /// </summary>
    public void MarkAsStarted()
    {
        CurrentRun.RecordStageTransition(JobStage.Running, "Job started");
    }

    /// <summary>
    /// Updates the job state when a retry is needed.
    /// </summary>
    /// <returns>True if the job can be retried; false if max retries have been reached.</returns>
    public bool PrepareForRetry()
    {
        // Check if we can retry before making any state changes
        if (CurrentRun.RetryCount >= JobDefinition.MaxRetries)
        {
            return false;
        }

        CurrentRun.RetryCount++;
        CurrentRun.LastRetryAt = DateTime.UtcNow;
        CurrentRun.RecordStageTransition(
            JobStage.Retrying,
            $"Retry attempt {CurrentRun.RetryCount} of {JobDefinition.MaxRetries}"
        );

        return true;
    }

    /// <summary>
    /// Marks the job as failed after exhausting all retry attempts.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    public void MarkAsFailed(Exception exception)
    {
        CurrentRun.RecordError(exception);
        CurrentRun.RecordStageTransition(
            JobStage.Failed,
            $"Failed after {CurrentRun.RetryCount} retry attempts"
        );
    }

    /// <summary>
    /// Marks the job as completed successfully.
    /// </summary>
    public void MarkAsCompleted()
    {
        CurrentRun.RecordStageTransition(JobStage.Completed, "Job completed successfully");
    }

    /// <summary>
    /// Marks the job as cancelled.
    /// </summary>
    /// <param name="reason">The reason for cancellation.</param>
    public void MarkAsCancelled(string reason)
    {
        CurrentRun.RecordStageTransition(JobStage.Cancelled, reason);
    }

    /// <summary>
    /// Prepares the job for compensation (rollback).
    /// </summary>
    public void PrepareForCompensation()
    {
        CurrentRun.RecordStageTransition(JobStage.Compensating, "Starting compensation process");
    }

    /// <summary>
    /// Marks the job as successfully compensated.
    /// </summary>
    public void MarkAsCompensated()
    {
        CurrentRun.RecordStageTransition(JobStage.Compensated, "Job successfully compensated");
    }

    /// <summary>
    /// Marks an error in the attempt and sets JobStage to Error
    /// </summary>
    internal void MarkAsErrored(Exception exception)
    {
        CurrentRun.RecordError(exception);
        CurrentRun.RecordStageTransition(
            JobStage.Error,
            $"Error on retry attempt {CurrentRun.RetryCount}"
        );
    }
}
