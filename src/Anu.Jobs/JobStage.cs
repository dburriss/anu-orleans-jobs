using System;

namespace Anu.Jobs
{
    /// <summary>
    /// Represents the possible stages of a job in its lifecycle.
    /// </summary>
    public enum JobStage
    {
        /// <summary>
        /// The job has been created but not yet scheduled for execution.
        /// </summary>
        Created,

        /// <summary>
        /// The job is scheduled and waiting to be executed.
        /// </summary>
        Scheduled,

        /// <summary>
        /// The job is currently being executed.
        /// </summary>
        Running,

        /// <summary>
        /// The job has completed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// The job has failed during execution.
        /// </summary>
        Failed,

        /// <summary>
        /// The job has been cancelled before completion.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The job is being retried after a failure.
        /// </summary>
        Retrying,

        /// <summary>
        /// The job is in the process of being compensated (rolled back).
        /// </summary>
        Compensating,

        /// <summary>
        /// The job has been successfully compensated.
        /// </summary>
        Compensated,

        /// <summary>
        /// The job has been suspended and is waiting for external intervention.
        /// </summary>
        Suspended
    }
}
