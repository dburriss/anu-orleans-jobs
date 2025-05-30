namespace Anu.Jobs;

/// <summary>
/// Represents the possible stages of a job in its lifecycle.
/// </summary>
public enum JobStage
{
    /// <summary>
    /// The job has been created but not yet scheduled for execution. Temporary state.
    /// </summary>
    Created,

    /// <summary>
    /// The job is scheduled and waiting to be executed. Temporary state.
    /// </summary>
    Scheduled,

    /// <summary>
    /// The job is currently being executed. Temporary state.
    /// </summary>
    Running,

    /// <summary>
    /// The job has completed successfully. Final state.
    /// </summary>
    Completed,

    /// <summary>
    /// The job has failed during execution. Temporary state.
    /// </summary>
    Failed,

    /// <summary>
    /// The job has been cancelled before completion.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The job is being retried after a failure. Temporary state.
    /// </summary>
    Retrying,

    /// <summary>
    /// The job is in the process of being compensated (rolled back). Temporary state.
    /// </summary>
    Compensating,

    /// <summary>
    /// The job has been successfully compensated. Final state.
    /// </summary>
    Compensated,

    /// <summary>
    /// The job has been suspended and is waiting for external intervention. Temporary or final state.
    /// </summary>
    Suspended,

    /// <summary>
    /// The job has errored out completely and could not even compensate. Final state.
    /// </summary>
    Error,

}
