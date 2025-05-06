using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anu.Jobs
{
    public class JobRunner : IJobRunner
    {
        public async Task<JobState> ExecuteJob(
            IJob job,
            JobState state,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                // Create job context from state
                var context = state.CreateJobContext(cancellationToken);

                // Record that we're starting execution
                state.MarkAsStarted();

                // Execute the job
                await job.Execute(context);

                // If we get here without exceptions, mark as completed
                state.MarkAsCompleted();
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation
                state.MarkAsCancelled("Job execution was cancelled");
            }
            catch (Exception ex)
            {
                // Handle failure
                state.MarkAsFailed(ex);

                // Note: We don't decide about retry here, we just return the updated state
                // The grain will decide if retry is appropriate
            }

            // Return the updated state to the caller (the grain)
            return state;
        }

        public async Task<JobState> ExecuteCompensation(
            IJob job,
            JobState state,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                // Create job context from state
                var context = state.CreateJobContext(cancellationToken);

                // Record that we're starting compensation
                state.PrepareForCompensation();

                // Execute the compensation logic
                await job.Compensate(context);

                // Mark as compensated
                state.MarkAsCompensated();
            }
            catch (Exception ex)
            {
                // Handle compensation failure
                state.CurrentRun.RecordError(ex);
            }

            // Return the updated state to the caller (the grain)
            return state;
        }
    }
}
