namespace Anu.Jobs;

public class TimerOptions
{
    /// <summary>
    /// Gets or sets the interval for the timer.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(60);

    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(60);
    public int MaxRetries { get; set; } = 3;
}
