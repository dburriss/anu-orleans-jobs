# Anu.Jobs Implementation Prompts

This document provides prompts for implementing the various components of the Anu.Jobs library.

## IJob Interface Implementation

```
Create the IJob interface for Anu.Jobs that defines the contract for job execution. The interface should:
1. Have an Execute method that takes an IJobContext and returns a Task
2. Have a Compensate method for handling failures
3. Include XML documentation
```

## Job Grain Implementation

```
Implement the IJobGrain interface and JobGrain class for Anu.Jobs. The grain should:
1. Support activating a job with a job definition
2. Track job status (pending, running, completed, failed)
3. Support cancellation
4. Support scheduling recurring execution using Orleans reminders
5. Handle failures and retries
6. Persist job state
7. Provide status reporting
```

## SiloBuilder Extensions

```
Create extension methods for ISiloBuilder to register and configure jobs:
1. UseJobs method to register job assemblies
2. UseRecurringJob method to register a recurring job with a specified interval
3. Support for configuring job execution parameters
4. Support for registering multiple jobs
```

## Job Context Implementation

```
Implement the IJobContext interface and JobContext class that provides:
1. Access to job metadata (ID, name, etc.)
2. Access to job parameters
3. Status information
4. Cancellation token
5. Methods for updating job status
```

## Job State Implementation

```
Create the JobState class for persisting job information:
1. Include job definition (type, name, etc.)
2. Track current status
3. Store execution history
4. Track retry information
5. Include serialization attributes for Orleans
```

## Lifecycle Participant Implementation

```
Implement the RegisterReminderLifecycleParticipant class that:
1. Registers with the Orleans lifecycle
2. Creates and configures job grains during silo startup
3. Sets up recurring jobs based on configuration
4. Handles cleanup during shutdown
```

## Unit Tests

```
Create unit tests for the Anu.Jobs library:
1. Test job activation and execution
2. Test recurring job scheduling
3. Test cancellation
4. Test failure handling and compensation
5. Test status reporting
6. Test SiloBuilder extensions
```

## Example Project

```
Create an example project demonstrating Anu.Jobs usage:
1. Define a simple recurring job that logs messages
2. Configure the job with the Orleans SiloBuilder
3. Show how to monitor job status
4. Demonstrate cancellation
5. Show how to handle failures
```
