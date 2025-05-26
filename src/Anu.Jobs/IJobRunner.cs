using System.Threading;
using System.Threading.Tasks;

namespace Anu.Jobs
{
    /// <summary>
    /// Defines the contract for executing jobs and their compensation logic.
    /// </summary>
    public interface IJobRunner
    {
        /// <summary>
        /// Executes a job and returns the updated state.
        /// </summary>
        /// <param name="job">The job instance to execute.</param>
        /// <param name="state">The current job state.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The updated job state after execution.</returns>
        Task<JobState> ExecuteJob(IJob job, JobState state, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Executes compensation logic for a failed job and returns the updated state.
        /// </summary>
        /// <param name="job">The job instance to compensate.</param>
        /// <param name="state">The current job state.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The updated job state after compensation.</returns>
        Task<JobState> ExecuteCompensation(IJob job, JobState state, CancellationToken cancellationToken = default);
    }
}
