using Orleans;

namespace Anu.Jobs;

[GenerateSerializer]
public class Triggers
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
