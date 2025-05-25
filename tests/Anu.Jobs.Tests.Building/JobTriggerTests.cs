namespace Anu.Jobs.BuildingTests;

public class JobTriggerTests
{
    [Fact]
    public void AddManualTrigger_SetsTypeToManual()
    {
        // Arrange
        var triggers = new JobTriggers();

        // Act
        var trigger = triggers.AddManualTrigger();

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
        var trigger = triggers.AddManualTrigger();
        var executionTime = DateTimeOffset.UtcNow;

        // Act
        triggers.UpdateLastExecution(trigger.Id, executionTime);

        // Assert
        Assert.Equal(executionTime, trigger.LastExecution);
    }
}
