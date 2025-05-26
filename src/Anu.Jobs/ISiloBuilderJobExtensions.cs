using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Anu.Jobs;

/// <summary>
/// Provides extension methods for configuring jobs in Orleans silos.
/// </summary>
public static class ISiloBuilderJobExtensions
{
    /// <summary>
    /// Adds job support to the Orleans silo by registering job types from the specified assemblies.
    /// </summary>
    /// <param name="builder">The silo builder.</param>
    /// <param name="jobAssemblies">The assemblies to scan for job types. If null, scans all loaded assemblies.</param>
    /// <returns>The silo builder for method chaining.</returns>
    public static ISiloBuilder AddJobs(this ISiloBuilder builder, params Assembly[] jobAssemblies)
    {
        var assemblies = jobAssemblies is null
            ? AppDomain.CurrentDomain.GetAssemblies()
            : AppDomain.CurrentDomain.GetAssemblies().Concat(jobAssemblies);
        var interfaceType = typeof(IJob);
        var jobTypes = assemblies
            .SelectMany(x => x.GetTypes())
            .Where(x => interfaceType.IsAssignableFrom(x))
            .Where(x => !x.IsInterface)
            .Where(x => !x.IsAbstract);

        builder.ConfigureServices(services =>
        {
            services.AddTransient<IJobBuilder, JobBuilder>();
            jobTypes.ToList().ForEach(x => services.AddTransient(x));
        });

        return builder;
    }

    /// <summary>
    /// Configures a job to run with the specified timer options.
    /// </summary>
    /// <param name="host">The silo builder.</param>
    /// <param name="jobType">The type of job to configure.</param>
    /// <param name="options">The timer options for the job.</param>
    /// <returns>The silo builder for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when interval is less than 1 minute for recurring jobs.</exception>
    public static ISiloBuilder UseJob(
        this ISiloBuilder host,
        Type jobType,
        TimerOptions options
    )
    {

        if (options.TimerType == TimerType.Recurring && options.Interval < TimeSpan.FromMinutes(1))
        {
            throw new ArgumentException(
                $"Interval must be at least 1 minute. Interval is {options.Interval}"
            );
        }
        host.ConfigureServices(services =>
            services.AddTransient<ILifecycleParticipant<ISiloLifecycle>>(sp =>
            {
                var jobName = jobType.Name;
                return new RegisterReminderLifecycleParticipant(
                    sp,
                    jobType,
                    jobName,
                    options
                );
            })
        );
        return host;
    }

    /// <summary>
    /// Configures a job to run on a recurring interval.
    /// </summary>
    /// <param name="host">The silo builder.</param>
    /// <param name="jobType">The type of job to configure.</param>
    /// <param name="interval">The interval between job executions.</param>
    /// <returns>The silo builder for method chaining.</returns>
    public static ISiloBuilder UseRecurringJob(
        this ISiloBuilder host,
        Type jobType,
        TimeSpan interval
    )
    {
        var intervalOptions = new TimerOptions { Interval = interval, TimerType = TimerType.Recurring };
        return host.UseJob(jobType, intervalOptions);
    }

    /// <summary>
    /// Configures a job of the specified type to run on a recurring interval.
    /// </summary>
    /// <typeparam name="TJob">The type of job to configure.</typeparam>
    /// <param name="host">The silo builder.</param>
    /// <param name="interval">The interval between job executions.</param>
    /// <returns>The silo builder for method chaining.</returns>
    public static ISiloBuilder UseRecurringJob<TJob>(this ISiloBuilder host, TimeSpan interval)
        where TJob : IJob
    {
        var jobType = typeof(TJob);
        return host.UseRecurringJob(jobType, interval);
    }

    /// <summary>
    /// Configures a job of the specified type to run once after an optional delay.
    /// </summary>
    /// <typeparam name="TJob">The type of job to configure.</typeparam>
    /// <param name="host">The silo builder.</param>
    /// <param name="delay">The delay before execution. Defaults to 60 seconds if not specified.</param>
    /// <returns>The silo builder for method chaining.</returns>
    public static ISiloBuilder UseOneTimeJob<TJob>(
        this ISiloBuilder host,
        TimeSpan? delay = null
    )
        where TJob : IJob
    {
        var jobType = typeof(TJob);
        var d = delay ?? TimeSpan.FromSeconds(60);
        var intervalOptions = new TimerOptions { Interval = TimeSpan.Zero, Delay = d };
        return host.UseJob(jobType, intervalOptions);
    }
}
