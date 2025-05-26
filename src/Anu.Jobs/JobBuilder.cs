using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anu.Jobs.Grains;
using Orleans;

namespace Anu.Jobs;

/// <summary>
/// Defines the contract for building and starting job instances.
/// </summary>
public interface IJobBuilder
{
    /// <summary>
    /// Starts a job of the specified type with an optional name.
    /// </summary>
    /// <param name="jobType">The type of job to start.</param>
    /// <param name="name">The optional name for the job. If null, uses the job type name.</param>
    /// <returns>The job grain instance.</returns>
    Task<IJobGrain> StartJobAsync(Type jobType, string? name);

    /// <summary>
    /// Starts a job of the specified generic type with an optional name.
    /// </summary>
    /// <typeparam name="TJob">The type of job to start.</typeparam>
    /// <param name="name">The optional name for the job. If null, uses the job type name.</param>
    /// <returns>The job grain instance.</returns>
    Task<IJobGrain> StartJobAsync<TJob>(string? name);
}

/// <summary>
/// Provides functionality for building and starting job instances.
/// </summary>
public class JobBuilder : IJobBuilder
{
    private readonly IGrainFactory _grainFactory;
    private readonly Func<string, string> _jobName;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobBuilder"/> class.
    /// </summary>
    /// <param name="grainFactory">The grain factory for creating job grains.</param>
    public JobBuilder(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
        _jobName = jobName => $"job__{jobName}";
    }

    /// <summary>
    /// Starts a job of the specified type with an optional name.
    /// </summary>
    /// <param name="jobType">The type of job to start.</param>
    /// <param name="name">The optional name for the job. If null, uses the job type name.</param>
    /// <returns>The job grain instance.</returns>
    public async Task<IJobGrain> StartJobAsync(Type jobType, string? name = null)
    {
        var jobName = name == null ? _jobName(jobType.Name) : _jobName(name);
        var jobGrain = _grainFactory.GetGrain<IJobGrain>(jobName);
        var jobDefinition = new JobDefinition(jobType, jobName, new Dictionary<string, object>());
        await jobGrain.Initialize(jobDefinition);
        return jobGrain;
    }

    /// <summary>
    /// Starts a job of the specified generic type with an optional name.
    /// </summary>
    /// <typeparam name="TJob">The type of job to start.</typeparam>
    /// <param name="name">The optional name for the job. If null, uses the job type name.</param>
    /// <returns>The job grain instance.</returns>
    public async Task<IJobGrain> StartJobAsync<TJob>(string? name = null)
    {
        return await StartJobAsync(typeof(TJob), name);
    }
}

/// <summary>
/// Provides extension methods for working with job grains.
/// </summary>
public static class JobBuilderExtensions
{
    /// <summary>
    /// Creates a job grain for the specified job type. Name defaults to the job type name.
    /// </summary>
    /// <typeparam name="TJob">The type of job to create.</typeparam>
    /// <param name="grainFactory">The grain factory.</param>
    /// <param name="name">The optional name for the job.</param>
    /// <returns>The created job grain instance.</returns>
    public static Task<IJobGrain> CreateJobGrain<TJob>(
        this IGrainFactory grainFactory,
        string? name = null
    )
    {
        var builder = new JobBuilder(grainFactory);
        return builder.StartJobAsync<TJob>();
    }

    /// <summary>
    /// Gets an existing job grain by name.
    /// </summary>
    /// <param name="grainFactory">The grain factory.</param>
    /// <param name="jobName">The name of the job.</param>
    /// <returns>The job grain instance.</returns>
    public static IJobGrain GetJobGrain(this IGrainFactory grainFactory, string jobName)
    {
        return grainFactory.GetGrain<IJobGrain>($"job__{jobName}");
    }
}
