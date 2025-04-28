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
- `UseRecurringJob<T>()` - Register a recurring job with a specified interval

### Job Triggers

Mechanisms to trigger job execution:

- Timer-based triggers (recurring jobs)
- Manual triggers

## Architecture

The architecture follows these principles:

1. **Grain-based execution** - Jobs are executed within Orleans grains for reliability and distribution
2. **Stateful tracking** - Job state is persisted within grain state
3. **Pluggable scheduling** - Support for different scheduling mechanisms
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
