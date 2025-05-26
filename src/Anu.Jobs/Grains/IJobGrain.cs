namespace Anu.Jobs.Grains;

/// <summary>
/// Defines the contract for a job grain that manages the lifecycle and execution of a job.
/// </summary>
public interface IJobGrain : IGrainWithStringKey, IRemindable
{
    /// <summary>
    /// Gets the current state of the job.
    /// </summary>
    /// <returns>The current job state.</returns>
    Task<JobState> GetState();

    /// <summary>
    /// Gets the definition of the job.
    /// </summary>
    /// <returns>The job definition.</returns>
    Task<JobDefinition> GetDefinition();

    /// <summary>
    /// Initializes the job grain with the specified job definition.
    /// </summary>
    /// <param name="definition">The job definition to initialize with.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Initialize(JobDefinition definition);

    /// <summary>
    /// Schedules the job for execution at the specified time.
    /// </summary>
    /// <param name="scheduledTime">The time to schedule execution. If null, schedules for immediate execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ScheduleExecution(DateTimeOffset? scheduledTime = null);

    /// <summary>
    /// Cancels the execution of the job with the specified reason.
    /// </summary>
    /// <param name="reason">The reason for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CancelExecution(string reason);

    /// <summary>
    /// Triggers immediate execution of the job. Called manually or by a reminder.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task TriggerExecution();

    /// <summary>
    /// Schedules the job for recurring execution with the specified period.
    /// </summary>
    /// <param name="period">The time period between executions.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ScheduleRecurringExecution(TimeSpan period);
}
