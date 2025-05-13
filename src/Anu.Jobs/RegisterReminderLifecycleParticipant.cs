using Microsoft.Extensions.DependencyInjection;

namespace Anu.Jobs;

// todo: investigate using startup task
internal class RegisterReminderLifecycleParticipant(
    IServiceProvider serviceProvider,
    Type jobType,
    string jobName,
    TimeSpan period
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
                // for each job, create a reminder
                var job = serviceProvider.GetService(jobType);
                if (job == null)
                    throw new InvalidOperationException(
                        $"Job {jobType.Name} not found in service collection."
                    );
                var grainFactory = serviceProvider.GetRequiredService<IJobBuilder>();
                var jobGrain = await grainFactory.StartJobAsync(job.GetType(), jobName);
                await jobGrain.ScheduleRecurringExecution(period);
            }
        );
    }
}
