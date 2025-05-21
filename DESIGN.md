# Anu.Jobs Design

## Overview

Anu.Jobs is a lightweight, Orleans-based job scheduling library that provides a simple way to define and execute background jobs in a distributed system.

## Core Components

### IJob Interface

The fundamental contract for defining jobs. Any class implementing this interface can be registered as a job.

```csharp
public interface IJob
{
    Task Execute(IJobContext context);
    Task Compensate(IJobContext context);
}
```

### Job Grain

An Orleans grain that manages the lifecycle of a job, including:

- Activation
- Execution
- Scheduling (one-time or recurring)
- Status tracking
- Cancellation
- Failure handling

### Job Context

Provides contextual information to the job during execution, such as:

- Job ID
- Execution parameters
- Status information
- Cancellation tokens

### SiloBuilder Extensions

Extension methods for the Orleans SiloBuilder to register and configure jobs:

- `UseJobs()` - Register job assemblies
- `AddRecurringJob<T>()` - Register a recurring job with a specified interval
- `AddOneTimeJob<T>()` - Register a job that runs  

### Job Triggers

Mechanisms to trigger job execution:

- Timer-based triggers (recurring jobs)
- Manual triggers
- Once-off scheduled
- Cron

Triggers are stored on the state of the grain. This is then used to determine the next time the reminder needs to trigger. Triggering will need to happen with the TriggerId.

## Architecture

The architecture follows these principles:

1. **Grain-based execution** - Jobs are executed within Orleans grains for reliability and distribution
2. **Stateful tracking** - Job state is persisted within grain state
3. **Flexible scheduling** - Support for different scheduling mechanisms e.g. manual run, single scheduled, recurring on period, cron
4. **Minimal dependencies** - Only depends on Orleans, not on the full Ez system

## Usage Flow

1. Define a job by implementing the `IJob` interface
2. Register the job with the Orleans SiloBuilder using extension methods
3. Configure job execution parameters (recurring interval, etc.)
4. The job grain manages the execution lifecycle

## Implementation Notes

- Job grains use Orleans reminders for scheduling recurring jobs
- Job state is persisted in grain state
- Jobs can be manually triggered or scheduled
- The library provides status tracking and reporting
