using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Anu.Jobs;

public static class ISiloBuilderJobExtensions
{
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

    public static ISiloBuilder UseRecurringJob(
        this ISiloBuilder host,
        Type jobType,
        TimeSpan interval
    )
    {
        var intervalOptions = new TimerOptions { Interval = interval, TimerType = TimerType.Recurring };
        return host.UseJob(jobType, intervalOptions);
    }

    public static ISiloBuilder UseRecurringJob<TJob>(this ISiloBuilder host, TimeSpan interval)
        where TJob : IJob
    {
        var jobType = typeof(TJob);
        return host.UseRecurringJob(jobType, interval);
    }

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
