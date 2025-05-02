using System.Threading;
using System.Threading.Tasks;

namespace Anu.Jobs
{
    public interface IJobRunner
    {
        // Execute a job and return the updated state
        Task<JobState> ExecuteJob(IJob job, JobState state, CancellationToken cancellationToken = default);
        
        // Execute compensation logic and return the updated state
        Task<JobState> ExecuteCompensation(IJob job, JobState state, CancellationToken cancellationToken = default);
    }
}
