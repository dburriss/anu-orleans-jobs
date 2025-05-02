using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

namespace Anu.Jobs.Grains
{
    public class JobGrain : Grain<JobState>, IJobGrain, IRemindable
    {
        private readonly IJobRunner _jobRunner;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobGrain> _logger;
        private const string ExecutionReminderName = "JobExecution";
        
        public JobGrain(
            IServiceProvider serviceProvider,
            ILogger<JobGrain> logger)
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
            return Task.FromResult(State.Definition);
        }
        
        public async Task Initialize(JobDefinition definition)
        {
            _logger.LogInformation("Initializing job grain for job type {JobType}", definition.JobType);
            
            State = new JobState
            {
                JobId = Guid.Parse(this.GetPrimaryKeyString()),
                Definition = definition,
                // Initialize other state properties
            };
            
            await WriteStateAsync();
        }
        
        public async Task ScheduleExecution(DateTime? scheduledTime = null)
        {
            // Calculate when to run the job
            var nextExecution = scheduledTime ?? State.Definition.GetNextExecutionTime();
            if (nextExecution.HasValue)
            {
                var delay = nextExecution.Value - DateTime.UtcNow;
                if (delay < TimeSpan.Zero)
                {
                    delay = TimeSpan.Zero;
                }
                
                _logger.LogInformation("Scheduling job {JobId} to run at {ExecutionTime}", 
                    State.JobId, nextExecution.Value);
                
                // Clear any existing reminder
                try
                {
                    await UnregisterReminder(await GetReminder(ExecutionReminderName));
                }
                catch (Exception ex) when (ex.Message.Contains("not found"))
                {
                    // Reminder doesn't exist, which is fine
                }
                
                // Register a new reminder
                await RegisterOrUpdateReminder(
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
                _logger.LogWarning("Could not determine next execution time for job {JobId}", State.JobId);
            }
        }
        
        public async Task CancelExecution(string reason)
        {
            _logger.LogInformation("Cancelling job {JobId}: {Reason}", State.JobId, reason);
            
            // Clear any existing reminder
            try
            {
                await UnregisterReminder(await GetReminder(ExecutionReminderName));
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                // Reminder doesn't exist, which is fine
            }
            
            State.MarkAsCancelled(reason);
            await WriteStateAsync();
        }
        
        public async Task TriggerExecution()
        {
            _logger.LogInformation("Triggering execution of job {JobId}", State.JobId);
            
            // Get the job implementation
            var jobType = Type.GetType(State.Definition.JobType);
            if (jobType == null)
            {
                _logger.LogError("Job type {JobType} not found", State.Definition.JobType);
                State.MarkAsFailed(new Exception($"Job type {State.Definition.JobType} not found"));
                await WriteStateAsync();
                return;
            }
            
            // Use IServiceProvider to get the job instance
            var job = _serviceProvider.GetService(jobType) as IJob;
            if (job == null)
            {
                _logger.LogError("Failed to create job instance of type {JobType}", State.Definition.JobType);
                State.MarkAsFailed(new Exception($"Failed to create job instance of type {State.Definition.JobType}"));
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
                if (State.CurrentStage == JobStage.Failed && State.PrepareForRetry())
                {
                    _logger.LogInformation("Scheduling retry for job {JobId}, attempt {Attempt}", 
                        State.JobId, State.Attempts);
                    
                    // Schedule a retry
                    var retryDelay = State.Definition.GetTrigger(State.CurrentTriggerId)?.CalculateRetryDelay(State.Attempts) 
                        ?? TimeSpan.FromMinutes(1); // Default retry delay
                    
                    await ScheduleExecution(DateTime.UtcNow.Add(retryDelay));
                }
                else if (State.CurrentStage == JobStage.Completed)
                {
                    _logger.LogInformation("Job {JobId} completed successfully", State.JobId);
                    
                    // If this is a recurring job, schedule the next execution
                    var nextExecution = State.Definition.GetNextExecutionTime(State.CompletedAt);
                    if (nextExecution.HasValue)
                    {
                        _logger.LogInformation("Scheduling next execution of recurring job {JobId}", State.JobId);
                        await ScheduleExecution(nextExecution.Value);
                    }
                }
                else if (State.CurrentStage == JobStage.Failed && State.Definition.ShouldCompensateOnFailure)
                {
                    _logger.LogInformation("Job {JobId} failed, executing compensation", State.JobId);
                    await ExecuteCompensation();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error executing job {JobId}", State.JobId);
                State.MarkAsFailed(ex);
                await WriteStateAsync();
            }
        }
        
        private async Task ExecuteCompensation()
        {
            // Get the job implementation
            var jobType = Type.GetType(State.Definition.JobType);
            if (jobType == null)
            {
                _logger.LogError("Cannot compensate: Job type {JobType} not found", State.Definition.JobType);
                return; // Already failed, just log this
            }
            
            // Use IServiceProvider to get the job instance
            var job = _serviceProvider.GetService(jobType) as IJob;
            if (job == null)
            {
                _logger.LogError("Cannot compensate: Failed to create job instance of type {JobType}", 
                    State.Definition.JobType);
                return; // Already failed, just log this
            }
            
            try
            {
                // Execute compensation using the runner
                var updatedState = await _jobRunner.ExecuteCompensation(job, State);
                State = updatedState;
                await WriteStateAsync();
                
                _logger.LogInformation("Compensation for job {JobId} {CompensationResult}", 
                    State.JobId, 
                    State.CurrentStage == JobStage.Compensated ? "completed successfully" : "failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during compensation for job {JobId}", State.JobId);
                State.RecordError(ex);
                await WriteStateAsync();
            }
        }
        
        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            _logger.LogInformation("Received reminder {ReminderName} for job {JobId}", reminderName, State.JobId);
            
            if (reminderName == ExecutionReminderName)
            {
                await TriggerExecution();
            }
        }
    }
}
