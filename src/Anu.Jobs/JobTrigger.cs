using Orleans;

namespace Anu.Jobs;

[GenerateSerializer]
public enum TriggerType
{
    Manual, // Job runs only when explicitly triggered
    OneTime, // Job runs once at a specific time
    Interval, // Job runs on a recurring interval
    Cron, // Job runs according to a cron expression
}

[GenerateSerializer]
public class JobTrigger
{
    // Unique identifier for this trigger
    [Id(0)]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Trigger type
    [Id(1)]
    public TriggerType Type { get; set; } = TriggerType.Manual;

    // Scheduling configuration
    [Id(2)]
    public DateTimeOffset? ScheduledTime { get; set; } // For OneTime

    [Id(3)]
    public TimeSpan? RecurringInterval { get; set; } // For Interval

    [Id(4)]
    public string? CronExpression { get; set; } // For Cron

    // Retry configuration
    [Id(5)]
    public int MaxRetries { get; set; } = 0;

    [Id(6)]
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    [Id(7)]
    public bool UseExponentialBackoff { get; set; } = false;

    // Last execution time for this trigger
    [Id(8)]
    public DateTimeOffset? LastExecution { get; set; }


    // Validation method
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

    // Calculate next execution time based on trigger type
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

    private DateTimeOffset? CalculateNextCronExecution(DateTimeOffset? lastExecution)
    {
        // Implementation would use a cron parser library to calculate the next run
        // based on the CronExpression
        // For now, return null as placeholder
        return null;
    }

    // Retry methods
    public bool CanRetry(int currentAttempt, Exception? lastException = null)
    {
        return currentAttempt < MaxRetries;
    }

    public TimeSpan CalculateRetryDelay(int attemptNumber)
    {
        if (UseExponentialBackoff)
        {
            return TimeSpan.FromTicks(RetryDelay.Ticks * (long)Math.Pow(2, attemptNumber - 1));
        }

        return RetryDelay;
    }

    // Static factory methods for common trigger types
    public static JobTrigger CreateManualTrigger(int maxRetries = 0)
    {
        return new JobTrigger { Type = TriggerType.Manual, MaxRetries = maxRetries };
    }

    public static JobTrigger CreateOneTimeTrigger(DateTime scheduledTime, int maxRetries = 0)
    {
        return new JobTrigger
        {
            Type = TriggerType.OneTime,
            ScheduledTime = scheduledTime,
            MaxRetries = maxRetries,
        };
    }

    public static JobTrigger CreateIntervalTrigger(TimeSpan interval, int maxRetries = 0)
    {
        return new JobTrigger
        {
            Type = TriggerType.Interval,
            RecurringInterval = interval,
            MaxRetries = maxRetries,
        };
    }

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

[GenerateSerializer]
public class JobTriggers
{
    [Id(0)]
    private List<JobTrigger> _triggers = new List<JobTrigger>();

    // Add a trigger to the collection
    public void Add(JobTrigger trigger)
    {
        _triggers.Add(trigger);
    }

    // Remove a trigger by its ID
    public bool Remove(Guid triggerId)
    {
        return _triggers.RemoveAll(t => t.Id == triggerId) > 0;
    }

    // Get a trigger by its ID
    public JobTrigger? Get(Guid triggerId)
    {
        return _triggers.Find(t => t.Id == triggerId);
    }

    // Get all triggers
    public IReadOnlyList<JobTrigger> GetAll()
    {
        return _triggers.AsReadOnly();
    }

    // Calculate the next execution time across all triggers
    public DateTimeOffset? GetNextExecutionTime(DateTimeOffset? lastExecution = null)
    {
        DateTimeOffset? nextTime = null;

        foreach (var trigger in _triggers)
        {
            var triggerNextTime = trigger.GetNextExecutionTime(lastExecution);

            if (triggerNextTime.HasValue &&
                (!nextTime.HasValue || triggerNextTime.Value < nextTime.Value))
            {
                nextTime = triggerNextTime;
            }
        }

        return nextTime;
    }

    // Update the last execution time for a specific trigger
    public void UpdateLastExecution(Guid triggerId, DateTimeOffset executionTime)
    {
        var trigger = Get(triggerId);
        if (trigger != null)
        {
            trigger.LastExecution = executionTime;
        }
    }
}
