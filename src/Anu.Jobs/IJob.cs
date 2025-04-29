namespace Anu.Jobs
{
    /// <summary>
    /// Defines the contract for job execution in the Anu.Jobs framework.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Executes the job with the provided context.
        /// </summary>
        /// <param name="context">The context containing information and services needed for job execution.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Execute(JobContext context);

        /// <summary>
        /// Compensates for a failed job execution by performing cleanup or rollback operations.
        /// </summary>
        /// <param name="context">The context containing information and services needed for compensation.</param>
        /// <returns>A task representing the asynchronous compensation operation.</returns>
        Task Compensate(JobContext context);
    }
}
