using Microsoft.Extensions.DependencyInjection;

namespace Anu.Jobs;

// todo: investigate using startup task
internal class RegisterReminderLifecycleParticipant(
    IServiceProvider serviceProvider,
    Type jobType,
    string jobName,
    TimerOptions options
) : ILifecycleParticipant<ISiloLifecycle>
{
    public void Participate(ISiloLifecycle lifecycle)
    {
        lifecycle.Subscribe(
            $"StartScheduledJob_{jobName}",
            ServiceLifecycleStage.Active,
            async token =>
            {
                Console.WriteLine($"Scheduling reminder for job {jobName}...");
                try
                {
                    if (options == null)
                        throw new ArgumentNullException(nameof(options));
                    // for each job, create a reminder
                    var job = serviceProvider.GetService(jobType);
                    if (job == null)
                        throw new InvalidOperationException(
                            $"Job {jobType.Name} not found in service collection."
                        );
                    var grainFactory = serviceProvider.GetRequiredService<IJobBuilder>();
                    var jobGrain = await grainFactory.StartJobAsync(job.GetType(), jobName);
                    if (options.TimerType == TimerType.OneTime)
                    {
                        await jobGrain.ScheduleExecution(DateTimeOffset.UtcNow + options.Delay);
                    }
                    // else
                    // {
                    //     await jobGrain.ScheduleRecurringExecution(options.Interval);
                    // }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lifecycle subscription failed. {ex.Message}");
                    throw;
                }
            }
        );
    }
}
