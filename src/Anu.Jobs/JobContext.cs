using System;
using System.Collections.Generic;
using System.Threading;

namespace Anu.Jobs
{
    /// <summary>
    /// Provides context information for job execution and compensation.
    /// </summary>
    public class JobContext
    {
        /// <summary>
        /// Gets or sets information about the current run of this job.
        /// </summary>
        public required JobRunInfo CurrentRun { get; set; }

        /// <summary>
        /// Gets or sets information about the previous run of this job, if any.
        /// This is useful for recurring jobs or jobs that need to reference their previous execution.
        /// </summary>
        public JobRunInfo? PreviousRun { get; set; }

        /// <summary>
        /// Gets or sets the input parameters for the job.
        /// </summary>
        public required Dictionary<string, object> InputParameters { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token for the job.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets a value indicating whether this job is being retried.
        /// </summary>
        public bool IsRetry => CurrentRun.RetryCount > 0;

        /// <summary>
        /// Gets a value indicating whether this job is a recurring execution.
        /// </summary>
        public bool IsRecurring => PreviousRun != null;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobContext"/> class.
        /// </summary>
        public JobContext()
        {
            InputParameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobContext"/> class with the specified run info and input parameters.
        /// </summary>
        /// <param name="currentRun">Information about the current run.</param>
        /// <param name="inputParameters">The input parameters for the job.</param>
        public JobContext(JobRunInfo currentRun, Dictionary<string, object> inputParameters)
        {
            CurrentRun = currentRun;
            InputParameters = inputParameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobContext"/> class with the specified 
        /// current run info, input parameters, and cancellation token.
        /// </summary>
        /// <param name="currentRun">Information about the current run.</param>
        /// <param name="inputParameters">The input parameters for the job.</param>
        /// <param name="cancellationToken">The cancellation token for the job.</param>
        public JobContext(JobRunInfo currentRun, Dictionary<string, object> inputParameters, CancellationToken cancellationToken)
            : this(currentRun, inputParameters)
        {
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets an input parameter value.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The parameter value.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the parameter doesn't exist.</exception>
        /// <exception cref="InvalidCastException">Thrown if the parameter can't be cast to the specified type.</exception>
        public T GetInputParameter<T>(string parameterName)
        {
            if (!InputParameters.TryGetValue(parameterName, out var value))
            {
                throw new KeyNotFoundException($"Input parameter '{parameterName}' not found.");
            }

            return (T)value;
        }

        /// <summary>
        /// Tries to get an input parameter value.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The output parameter value if found and of the correct type.</param>
        /// <returns>True if the parameter was found and is of the correct type; otherwise, false.</returns>
        public bool TryGetInputParameter<T>(string parameterName, out T value)
        {
            value = default!;

            if (!InputParameters.TryGetValue(parameterName, out var paramValue))
            {
                return false;
            }

            if (paramValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            return false;
        }
    }
}
