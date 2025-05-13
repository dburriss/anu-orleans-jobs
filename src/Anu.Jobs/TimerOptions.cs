namespace Anu.Jobs;

public class TimerOptions
{
    /// <summary>
    /// Gets or sets the interval for the timer.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);

    public int MaxRetries { get; set; } = 3;
}
