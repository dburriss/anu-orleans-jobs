namespace Anu.Jobs.BuildingTests;

public class JobTriggerTests
{
    [Fact]
    public void CreateManualTrigger_SetsTypeToManual()
    {
        // Act
        var trigger = JobTrigger.CreateManualTrigger();

        // Assert
        Assert.Equal(TriggerType.Manual, trigger.Type);
        Assert.Equal(0, trigger.MaxRetries);
        Assert.Null(trigger.ScheduledTime);
        Assert.Null(trigger.RecurringInterval);
        Assert.Null(trigger.CronExpression);
    }

    [Fact]
    public void UpdateLastExecution_SetsLastExecutionTime()
    {
        // Arrange
        var triggers = new JobTriggers();
        var trigger = JobTrigger.CreateManualTrigger();
        var executionTime = DateTimeOffset.UtcNow;
        
        triggers.Add(trigger);

        // Act
        triggers.UpdateLastExecution(trigger.Id, executionTime);

        // Assert
        Assert.Equal(executionTime, trigger.LastExecution);
    }
}
