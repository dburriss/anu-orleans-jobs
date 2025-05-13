using System;
using System.Threading.Tasks;
using Anu.Jobs;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

namespace Anu.Jobs.Grains;

public class JobGrain : Grain<JobState>, IJobGrain, IRemindable
{
    private readonly IJobRunner _jobRunner;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobGrain> _logger;
    private const string ExecutionReminderName = "JobExecution";

    public JobGrain(IServiceProvider serviceProvider, ILogger<JobGrain> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _jobRunner = new JobRunner();
    }

    public Task<JobState> GetState()
    {
        return Task.FromResult(State);
    }

    public Task<JobDefinition> GetDefinition()
    {
        return Task.FromResult(State.JobDefinition);
    }

    public async Task Initialize(JobDefinition definition)
    {
        _logger.LogInformation("Initializing job grain for job type {JobType}", definition.JobType);

        State = new JobState
        {
            JobDefinition = definition,
            // Initialize other state properties
        };

        await WriteStateAsync();
    }

    public async Task TriggerExecution()
    {
        _logger.LogInformation(
            "Triggering execution of job {JobName}",
            State.JobDefinition.JobName
        );

        // Get the job implementation
        var jobType = State.JobDefinition.JobType;
        if (jobType == null)
        {
            _logger.LogError("Job type {JobType} not found", State.JobDefinition.JobType);
            State.MarkAsFailed(new Exception($"Job type {State.JobDefinition.JobType} not found"));
            await WriteStateAsync();
            return;
        }

        // Use IServiceProvider to get the job instance
        var job = _serviceProvider.GetService(jobType) as IJob;
        if (job == null)
        {
            _logger.LogError(
                "Failed to create job instance of type {JobType}",
                State.JobDefinition.JobType
            );
            State.MarkAsFailed(
                new Exception(
                    $"Failed to create job instance of type {State.JobDefinition.JobType}"
                )
            );
            await WriteStateAsync();
            return;
        }

        // Execute the job using the runner
        try
        {
            var updatedState = await _jobRunner.ExecuteJob(job, State);
            State = updatedState;
            await WriteStateAsync();

            // Handle the result based on the updated state
            if (State.CurrentRun.Stage == JobStage.Failed && State.PrepareForRetry())
            {
                _logger.LogInformation(
                    "Scheduling retry for job {JobName}, attempt {Attempt}",
                    State.JobDefinition.JobName,
                    State.CurrentRun.RetryCount
                );

                // Schedule a retry
                var retryDelay =
                    State
                        .JobDefinition.GetTrigger(State.CurrentRun.TriggerId)
                        ?.CalculateRetryDelay(State.CurrentRun.RetryCount)
                    ?? TimeSpan.FromMinutes(1); // Default retry delay

                await ScheduleExecution(DateTime.UtcNow.Add(retryDelay));
            }
            else if (State.CurrentRun.Stage == JobStage.Completed)
            {
                _logger.LogInformation(
                    "Job {JobName} completed successfully",
                    State.JobDefinition.JobName
                );

                // If this is a recurring job, schedule the next execution
                var nextExecution = State.JobDefinition.GetNextExecutionTime(
                    State.CurrentRun.CompletedAt
                );
                if (nextExecution.HasValue)
                {
                    _logger.LogInformation(
                        "Scheduling next execution of recurring job {JobId}",
                        State.JobDefinition.JobName
                    );
                    await ScheduleExecution(nextExecution.Value);
                }
            }
            else if (
                State.CurrentRun.Stage == JobStage.Failed
                && (State.CurrentRun.RetryCount >= State.JobDefinition.MaxRetries)
            )
            {
                _logger.LogInformation(
                    "Job {JobName} failed, executing compensation",
                    State.JobDefinition.JobName
                );
                await ExecuteCompensation();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error executing job {JobName}",
                State.JobDefinition.JobName
            );
            State.MarkAsFailed(ex);
            await WriteStateAsync();
        }
    }

    private async Task ExecuteCompensation()
    {
        // Get the job implementation
        var jobType = State.JobDefinition.JobType;
        if (jobType == null)
        {
            _logger.LogError(
                "Cannot compensate: Job type {JobType} not found",
                State.JobDefinition.JobType
            );
            return; // Already failed, just log this
        }

        // Use IServiceProvider to get the job instance
        var job = _serviceProvider.GetService(jobType) as IJob;
        if (job == null)
        {
            _logger.LogError(
                "Cannot compensate: Failed to create job instance of type {JobType}",
                State.JobDefinition.JobType
            );
            return; // Already failed, just log this
        }

        try
        {
            // Execute compensation using the runner
            var updatedState = await _jobRunner.ExecuteCompensation(job, State);
            State = updatedState;
            await WriteStateAsync();

            _logger.LogInformation(
                "Compensation for job {JobName} {CompensationResult}",
                State.JobDefinition.JobName,
                State.CurrentRun.Stage == JobStage.Compensated ? "completed successfully" : "failed"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error during compensation for job {JobId}",
                State.JobDefinition.JobName
            );
            _logger.LogError(
                "Compensation for job {JobName} failed: {Error}",
                State.JobDefinition.JobName,
                ex.Message
            );
            await WriteStateAsync();
        }
    }

    public async Task ScheduleExecution(DateTime? scheduledTime = null)
    {
        // Calculate when to run the job
        var nextExecution = scheduledTime ?? State.JobDefinition.GetNextExecutionTime();
        if (nextExecution.HasValue)
        {
            var delay = nextExecution.Value - DateTime.UtcNow;
            if (delay < TimeSpan.Zero)
            {
                delay = TimeSpan.Zero;
            }

            _logger.LogInformation(
                "Scheduling job {JobName} to run at {ExecutionTime}",
                this.GetPrimaryKey(),
                nextExecution.Value
            );

            // Clear any existing reminder
            try
            {
                var reminder = await this.GetReminder(ExecutionReminderName);
                if (reminder != null)
                    await this.UnregisterReminder(reminder);
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                // Reminder doesn't exist, which is fine
            }

            // Register a new reminder
            await this.RegisterOrUpdateReminder(
                ExecutionReminderName,
                delay,
                TimeSpan.FromMilliseconds(-1) // Don't repeat
            );

            // Update state
            State.ScheduledTime = nextExecution.Value;
            await WriteStateAsync();
        }
        else
        {
            _logger.LogWarning(
                "Could not determine next execution time for job {JobName}",
                this.GetPrimaryKey()
            );
        }
    }

    public async Task ScheduleRecurringExecution(TimeSpan period)
    {
        _logger.LogInformation(
            "Scheduling recurring execution on period {Period} for job {JobName}",
            period,
            State.JobDefinition.JobName
        );

        await this.RegisterOrUpdateReminder(ExecutionReminderName, TimeSpan.Zero, period);
    }

    public async Task CancelExecution(string reason)
    {
        _logger.LogInformation(
            "Cancelling job {JobName}: {Reason}",
            State.JobDefinition.JobName,
            reason
        );

        // Clear any existing reminder
        try
        {
            var reminder = await this.GetReminder(ExecutionReminderName);
            if (reminder != null)
                await this.UnregisterReminder(reminder);
        }
        catch (Exception ex) when (ex.Message.Contains("not found"))
        {
            // Reminder doesn't exist, which is fine
        }

        State.MarkAsCancelled(reason);
        await WriteStateAsync();
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        _logger.LogInformation(
            "Received reminder {ReminderName} for job {JobName}",
            reminderName,
            State.JobDefinition.JobName
        );

        if (reminderName == ExecutionReminderName)
        {
            await TriggerExecution();
        }
    }
}
