using System;
using System.Collections.Generic;
using Orleans;

namespace Anu.Jobs
{
    /// <summary>
    /// Defines a job to be executed, including its name, type, and input parameters.
    /// This class is designed to be serializable for storage and transmission.
    /// </summary>
    [GenerateSerializer]
    public class JobDefinition
    {
        /// <summary>
        /// Gets or sets the name of the job.
        /// </summary>
        [Id(0)]
        public string JobName { get; set; }

        /// <summary>
        /// Gets or sets the type of the job.
        /// This should be the fully qualified name of a class implementing IJob.
        /// </summary>
        [Id(1)]
        public string JobType { get; set; }

        /// <summary>
        /// Gets or sets the input parameters for the job.
        /// </summary>
        [Id(2)]
        public Dictionary<string, object> InputParameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Creates a new instance of the JobDefinition class.
        /// </summary>
        public JobDefinition()
        {
        }

        /// <summary>
        /// Creates a new instance of the JobDefinition class with the specified name and type.
        /// </summary>
        /// <param name="jobName">The name of the job.</param>
        /// <param name="jobType">The type of the job.</param>
        public JobDefinition(string jobName, string jobType)
        {
            JobName = jobName;
            JobType = jobType;
        }

        /// <summary>
        /// Creates a new instance of the JobDefinition class with the specified name, type, and input parameters.
        /// </summary>
        /// <param name="jobName">The name of the job.</param>
        /// <param name="jobType">The type of the job.</param>
        /// <param name="inputParameters">The input parameters for the job.</param>
        public JobDefinition(string jobName, string jobType, Dictionary<string, object> inputParameters)
        {
            JobName = jobName;
            JobType = jobType;
            InputParameters = inputParameters ?? new Dictionary<string, object>();
        }
    }
}
