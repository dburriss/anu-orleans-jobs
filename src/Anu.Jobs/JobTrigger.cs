using Orleans;

namespace Anu.Jobs;

/// <summary>
/// Defines the types of triggers that can initiate job execution.
/// </summary>
[GenerateSerializer]
public enum TriggerType
{
    /// <summary>
    /// Job runs only when explicitly triggered.
    /// </summary>
    Manual,
    
    /// <summary>
    /// Job runs once at a specific time.
    /// </summary>
    OneTime,
    
    /// <summary>
    /// Job runs on a recurring interval.
    /// </summary>
    Interval,
    
    /// <summary>
    /// Job runs according to a cron expression.
    /// </summary>
    Cron,
}

/// <summary>
/// Represents a trigger that can initiate job execution with scheduling and retry configuration.
/// </summary>
[GenerateSerializer]
public class JobTrigger
{
    /// <summary>
    /// Gets or sets the unique identifier for this trigger.
    /// </summary>
    [Id(0)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the trigger type.
    /// </summary>
    [Id(1)]
    public TriggerType Type { get; set; } = TriggerType.Manual;

    /// <summary>
    /// Gets or sets the scheduled time for one-time triggers.
    /// </summary>
    [Id(2)]
    public DateTimeOffset? ScheduledTime { get; set; }

    /// <summary>
    /// Gets or sets the recurring interval for interval triggers.
    /// </summary>
    [Id(3)]
    public TimeSpan? RecurringInterval { get; set; }

    /// <summary>
    /// Gets or sets the cron expression for cron triggers.
    /// </summary>
    [Id(4)]
    public string? CronExpression { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for this trigger.
    /// </summary>
    [Id(5)]
    public int MaxRetries { get; set; } = 0;

    /// <summary>
    /// Gets or sets the delay between retry attempts.
    /// </summary>
    [Id(6)]
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets a value indicating whether to use exponential backoff for retries.
    /// </summary>
    [Id(7)]
    public bool UseExponentialBackoff { get; set; } = false;

    /// <summary>
    /// Gets or sets the last execution time for this trigger.
    /// </summary>
    [Id(8)]
    public DateTimeOffset? LastExecution { get; set; }

    /// <summary>
    /// Validates whether this trigger is properly configured.
    /// </summary>
    /// <returns>True if the trigger is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        return Type switch
        {
            TriggerType.Manual => true,
            TriggerType.OneTime => ScheduledTime.HasValue,
            TriggerType.Interval => RecurringInterval.HasValue,
            TriggerType.Cron => !string.IsNullOrEmpty(CronExpression),
            _ => false,
        };
    }

    /// <summary>
    /// Calculates the next execution time based on the trigger type.
    /// </summary>
    /// <param name="lastExecution">The last execution time to calculate from.</param>
    /// <returns>The next execution time, or null if not applicable.</returns>
    public DateTimeOffset? GetNextExecutionTime(DateTimeOffset? lastExecution = null)
    {
        return Type switch
        {
            TriggerType.Manual => null, // Manual triggers don't schedule automatically
            TriggerType.OneTime => ScheduledTime,
            TriggerType.Interval => lastExecution.HasValue
                ? lastExecution.Value.Add(RecurringInterval.Value)
                : DateTimeOffset.UtcNow.Add(RecurringInterval.Value),
            TriggerType.Cron => CalculateNextCronExecution(lastExecution),
            _ => null,
        };
    }

    /// <summary>
    /// Calculates the next execution time for cron triggers.
    /// </summary>
    /// <param name="lastExecution">The last execution time.</param>
    /// <returns>The next execution time, or null if not calculable.</returns>
    private DateTimeOffset? CalculateNextCronExecution(DateTimeOffset? lastExecution)
    {
        // Implementation would use a cron parser library to calculate the next run
        // based on the CronExpression
        // For now, return null as placeholder
        return null;
    }

    /// <summary>
    /// Determines whether the trigger can retry after a failure.
    /// </summary>
    /// <param name="currentAttempt">The current attempt number.</param>
    /// <param name="lastException">The last exception that occurred (optional).</param>
    /// <returns>True if retry is allowed; otherwise, false.</returns>
    public bool CanRetry(int currentAttempt, Exception? lastException = null)
    {
        return currentAttempt < MaxRetries;
    }

    /// <summary>
    /// Calculates the delay before the next retry attempt.
    /// </summary>
    /// <param name="attemptNumber">The attempt number (1-based).</param>
    /// <returns>The delay before the next retry.</returns>
    public TimeSpan CalculateRetryDelay(int attemptNumber)
    {
        if (UseExponentialBackoff)
        {
            return TimeSpan.FromTicks(RetryDelay.Ticks * (long)Math.Pow(2, attemptNumber - 1));
        }

        return RetryDelay;
    }

    /// <summary>
    /// Creates a manual trigger that runs only when explicitly triggered.
    /// </summary>
    /// <param name="maxRetries">The maximum number of retry attempts.</param>
    /// <returns>A new manual trigger.</returns>
    public static JobTrigger CreateManualTrigger(int maxRetries = 0)
    {
        return new JobTrigger { Type = TriggerType.Manual, MaxRetries = maxRetries };
    }

    /// <summary>
    /// Creates a one-time trigger that runs at a specific time.
    /// </summary>
    /// <param name="scheduledTime">The time to schedule execution.</param>
    /// <param name="maxRetries">The maximum number of retry attempts.</param>
    /// <returns>A new one-time trigger.</returns>
    public static JobTrigger CreateOneTimeTrigger(DateTime scheduledTime, int maxRetries = 0)
    {
        return new JobTrigger
        {
            Type = TriggerType.OneTime,
            ScheduledTime = scheduledTime,
            MaxRetries = maxRetries,
        };
    }

    /// <summary>
    /// Creates an interval trigger that runs on a recurring schedule.
    /// </summary>
    /// <param name="interval">The interval between executions.</param>
    /// <param name="maxRetries">The maximum number of retry attempts.</param>
    /// <returns>A new interval trigger.</returns>
    public static JobTrigger CreateIntervalTrigger(TimeSpan interval, int maxRetries = 0)
    {
        return new JobTrigger
        {
            Type = TriggerType.Interval,
            RecurringInterval = interval,
            MaxRetries = maxRetries,
        };
    }

    /// <summary>
    /// Creates a cron trigger that runs according to a cron expression.
    /// </summary>
    /// <param name="cronExpression">The cron expression defining the schedule.</param>
    /// <param name="maxRetries">The maximum number of retry attempts.</param>
    /// <returns>A new cron trigger.</returns>
    public static JobTrigger CreateCronTrigger(string cronExpression, int maxRetries = 0)
    {
        return new JobTrigger
        {
            Type = TriggerType.Cron,
            CronExpression = cronExpression,
            MaxRetries = maxRetries,
        };
    }
}
