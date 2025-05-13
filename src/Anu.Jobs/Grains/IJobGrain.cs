namespace Anu.Jobs.Grains;

public interface IJobGrain : IGrainWithStringKey, IRemindable
{
    Task<JobState> GetState();
    Task<JobDefinition> GetDefinition();

    // Job lifecycle management
    Task Initialize(JobDefinition definition);
    Task ScheduleExecution(DateTime? scheduledTime = null);
    Task ScheduleRecurringExecution(TimeSpan period);
    Task CancelExecution(string reason);
    Task TriggerExecution(); // Called manually or by a reminder
}
