namespace Anu.Jobs
{
    /// <summary>
    /// Provides context information for job execution and compensation.
    /// </summary>
    public class JobContext
    {
        /// <summary>
        /// Gets or sets the name of the job.
        /// </summary>
        public required string JobName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for this job run.
        /// </summary>
        public required string RunId { get; set; }

        /// <summary>
        /// Gets or sets the time when the job started.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token for the job.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobContext"/> class.
        /// </summary>
        public JobContext()
        {
            RunId = Guid.NewGuid().ToString();
            StartTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobContext"/> class with a specified job name.
        /// </summary>
        /// <param name="jobName">The name of the job.</param>
        public JobContext(string jobName)
            : this()
        {
            JobName = jobName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobContext"/> class with a specified job name and cancellation token.
        /// </summary>
        /// <param name="jobName">The name of the job.</param>
        /// <param name="cancellationToken">The cancellation token for the job.</param>
        public JobContext(string jobName, CancellationToken cancellationToken)
            : this(jobName)
        {
            CancellationToken = cancellationToken;
        }
    }
}
