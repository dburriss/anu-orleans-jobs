namespace Anu.Jobs.Tests.Building;

/// <summary>
/// Building tests for JobState.CanRetry() method.
/// </summary>
public class JobStateCanRetryTests
{
    [Fact]
    public void WhenRetryCountIsZeroAndMaxRetriesIsZero_ThenReturnsFalse()
    {
        // Arrange
        var jobState = CreateJobState(maxRetries: 0);
        jobState.CurrentRun.RetryCount = 0;

        // Act
        var result = jobState.CanRetry();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WhenRetryCountIsZeroAndMaxRetriesIsOne_ThenReturnsTrue()
    {
        // Arrange
        var jobState = CreateJobState(maxRetries: 1);
        jobState.CurrentRun.RetryCount = 0;

        // Act
        var result = jobState.CanRetry();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WhenRetryCountEqualsMaxRetries_ThenReturnsFalse()
    {
        // Arrange
        var jobState = CreateJobState(maxRetries: 3);
        jobState.CurrentRun.RetryCount = 3;

        // Act
        var result = jobState.CanRetry();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WhenRetryCountIsLessThanMaxRetries_ThenReturnsTrue()
    {
        // Arrange
        var jobState = CreateJobState(maxRetries: 5);
        jobState.CurrentRun.RetryCount = 2;

        // Act
        var result = jobState.CanRetry();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WhenRetryCountExceedsMaxRetries_ThenReturnsFalse()
    {
        // Arrange
        var jobState = CreateJobState(maxRetries: 2);
        jobState.CurrentRun.RetryCount = 3; // This shouldn't happen in normal flow, but test edge case

        // Act
        var result = jobState.CanRetry();

        // Assert
        Assert.False(result);
    }

    private static JobState CreateJobState(int maxRetries)
    {
        return new JobState
        {
            JobDefinition = new JobDefinition(typeof(object), "TestJob")
            {
                MaxRetries = maxRetries
            }
        };
    }
}
