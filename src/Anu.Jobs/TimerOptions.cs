namespace Anu.Jobs;

public class TimerOptions
{
    /// <summary>
    /// Gets or sets the interval for a recurring timer. Ignored for a one-time timer.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(60);

    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(60);
    public int MaxRetries { get; set; } = 3;
    public TimerType TimerType { get; set; } = TimerType.OneTime;
}

public enum TimerType
{
    /// <summary>
    /// The timer will run once after the specified delay.
    /// </summary>
    OneTime,

    /// <summary>
    /// The timer will run at the specified interval.
    /// </summary>
    Recurring,
}
