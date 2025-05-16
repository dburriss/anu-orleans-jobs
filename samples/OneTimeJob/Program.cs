using Anu.Jobs;
using Orleans.Runtime;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(static siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorageAsDefault();
    siloBuilder.UseInMemoryReminderService();

    siloBuilder.AddJobs(typeof(OneTimeJob).Assembly);
    siloBuilder.UseOneTimeJob<OneTimeJob>();
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet(
        "/jobs",
        () =>
        {
            return Results.Ok("Hello from OneTimeJob!");
        }
    )
    .WithName("GetJobs")
    .WithOpenApi();

app.Run();

public class OneTimeJob : IJob
{
    public Task Execute(JobContext context)
    {
        Console.WriteLine("Executing OneTimeJob...");
        return Task.CompletedTask;
    }

    public Task Compensate(JobContext context)
    {
        Console.WriteLine("Compensating OneTimeJob...");
        return Task.CompletedTask;
    }
}
