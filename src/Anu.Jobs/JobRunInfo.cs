using System;
using System.Collections.Generic;
using Orleans;

namespace Anu.Jobs
{
    /// <summary>
    /// Information about a specific execution of a job.
    /// </summary>
    [GenerateSerializer]
    public class JobRunInfo
    {
        /// <summary>
        /// Unique identifier for this execution.
        /// </summary>
        [Id(0)]
        public Guid RunId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Identifier for what triggered this job.
        /// </summary>
        [Id(1)]
        public Guid TriggerId { get; set; }

        /// <summary>
        /// Type of trigger that initiated this job.
        /// </summary>
        [Id(2)]
        public TriggerType TriggerType { get; set; } = TriggerType.Manual;

        /// <summary>
        /// Current stage of this run.
        /// </summary>
        [Id(3)]
        public JobStage Stage { get; set; } = JobStage.Created;

        /// <summary>
        /// Number of retry attempts for this run.
        /// </summary>
        [Id(4)]
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// When the last retry was attempted.
        /// </summary>
        [Id(5)]
        public DateTime? LastRetryAt { get; set; }

        /// <summary>
        /// When the job was created/scheduled.
        /// </summary>
        [Id(6)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the job started executing.
        /// </summary>
        [Id(7)]
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// When the job was last updated.
        /// </summary>
        [Id(8)]
        public DateTime? LastUpdatedAt { get; set; }

        /// <summary>
        /// When the job completed (successfully or not).
        /// </summary>
        [Id(9)]
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Information about any error that occurred.
        /// </summary>
        [Id(10)]
        public ExceptionInfo? Error { get; set; }

        /// <summary>
        /// History of stage transitions with timestamps.
        /// </summary>
        [Id(11)]
        public List<StageTransition> StageHistory { get; set; } = new List<StageTransition>();

        /// <summary>
        /// Records a stage transition in the job's history.
        /// </summary>
        public void RecordStageTransition(JobStage newStage, string? reason = null)
        {
            var transition = new StageTransition
            {
                PreviousStage = Stage,
                NewStage = newStage,
                TransitionTime = DateTime.UtcNow,
                Reason = reason
            };

            StageHistory.Add(transition);
            Stage = newStage;
            LastUpdatedAt = transition.TransitionTime;

            // Set specific timestamps based on the stage
            switch (newStage)
            {
                case JobStage.Running:
                    if (!StartedAt.HasValue)
                        StartedAt = transition.TransitionTime;
                    break;
                case JobStage.Completed:
                case JobStage.Failed:
                case JobStage.Cancelled:
                case JobStage.Compensated:
                    CompletedAt = transition.TransitionTime;
                    break;
            }
        }

        /// <summary>
        /// Records an error that occurred during job execution.
        /// </summary>
        public void RecordError(Exception exception)
        {
            Error = new ExceptionInfo(exception);
            LastUpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Represents a transition between job stages.
    /// </summary>
    [GenerateSerializer]
    public class StageTransition
    {
        /// <summary>
        /// The stage before the transition.
        /// </summary>
        [Id(0)]
        public JobStage PreviousStage { get; set; }

        /// <summary>
        /// The stage after the transition.
        /// </summary>
        [Id(1)]
        public JobStage NewStage { get; set; }

        /// <summary>
        /// When the transition occurred.
        /// </summary>
        [Id(2)]
        public DateTime TransitionTime { get; set; }

        /// <summary>
        /// Optional reason for the transition.
        /// </summary>
        [Id(3)]
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Types of triggers that can initiate a job.
    /// </summary>
    public enum TriggerType
    {
        /// <summary>
        /// Job was manually triggered.
        /// </summary>
        Manual,

        /// <summary>
        /// Job was triggered by a schedule.
        /// </summary>
        Scheduled,

        /// <summary>
        /// Job was triggered by the completion of another job.
        /// </summary>
        DependencyCompleted,

        /// <summary>
        /// Job was triggered by a system event.
        /// </summary>
        SystemInitiated
    }
}
