using ApiGateway;
using Grpc.Net.Client;
using Performance;
using System.Diagnostics;
using Monitor = Performance.Monitor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddSingleton<IGrpcPerformanceClient>(p =>
    new GrpcPerformanceClient(builder.Configuration["ServerUrl"]));

builder.Services.AddGrpcClient<Monitor.MonitorClient>(o =>
    o.Address = new Uri(builder.Configuration["ServerUrl"]));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("factory-client/{count}", async (int count, Monitor.MonitorClient factoryClient) =>
{
    var stopWatch = Stopwatch.StartNew();
    var response = new ResponseModel();
    for (var i = 0; i < count; i++)
    {
        var grpcResponse = await factoryClient.GetPerformanceAsync(new PerformanceStatusRequest
        {
            ClientName = $"client {i + 1}"
        });

        response.PerformanceStatuses.Add(new ResponseModel.PerformanceStatusModel
        {
            CpuPercentageUsage = grpcResponse.CpuPercentageUsage,
            MemoryUsage = grpcResponse.MemoryUsage,
            ProcessesRunning = grpcResponse.ProcessesRunning,
            ActiveConnections = grpcResponse.ActiveConnections
        });
    }
    response.RequestProcessingTime = stopWatch.ElapsedMilliseconds;
    return response;
})
.WithName("GetPerformanceFromFactoryClient")
.WithOpenApi();

app.MapGet("client-wrapper/{count}", async (int count, IGrpcPerformanceClient clientWrapper) =>
{
    var stopWatch = Stopwatch.StartNew();
    var response = new ResponseModel();
    for (var i = 0; i < count; i++)
    {
        var grpcResponse = await clientWrapper.GetPerformanceStatus($"client {i + 1}");
        response.PerformanceStatuses.Add(grpcResponse);
    }
    response.RequestProcessingTime = stopWatch.ElapsedMilliseconds;
    return response;
})
.WithName("GetPerformanceFromClientWrapper")
.WithOpenApi();

string serverUrl = builder.Configuration["ServerUrl"];

app.MapGet("initialized-client/{count}", async (int count) =>
{
    var stopWatch = Stopwatch.StartNew();
    var response = new ResponseModel();
    for (var i = 0; i < count; i++)
    {
        using var channel = GrpcChannel.ForAddress(serverUrl);
        var client = new Monitor.MonitorClient(channel);
        var grpcResponse = await client.GetPerformanceAsync(new PerformanceStatusRequest
        {
            ClientName = $"client {i + 1}"
        });
        response.PerformanceStatuses.Add(new ResponseModel.PerformanceStatusModel
        {
            CpuPercentageUsage = grpcResponse.CpuPercentageUsage,
            MemoryUsage = grpcResponse.MemoryUsage,
            ProcessesRunning = grpcResponse.ProcessesRunning,
            ActiveConnections = grpcResponse.ActiveConnections
        });
    }
    response.RequestProcessingTime = stopWatch.ElapsedMilliseconds;
    return response;
})
.WithName("GetPerformanceFromNewClient")
.WithOpenApi();

app.MapGet("single-connection/{count}", (int count) => 
{
    using var channel = GrpcChannel.ForAddress(serverUrl);
    var stopWatch = Stopwatch.StartNew();
    var response = new ResponseModel();
    var concurrentJobs = new List<Task>();
    for (var i = 0; i < count; i++)
    {
        var client = new Monitor.MonitorClient(channel);
        concurrentJobs.Add(Task.Run(() => 
        {
            client.GetPerformance(new PerformanceStatusRequest
            {
                ClientName = $"client {i + 1}"
            });
        }));
    }
    Task.WaitAll(concurrentJobs.ToArray());
    response.RequestProcessingTime = stopWatch.ElapsedMilliseconds;
    return response;
})
.WithName("GetDataFromSingleConnection")
.WithOpenApi();

app.MapGet("multiple-connections/{count}", (int count) =>
{
    using var channel = GrpcChannel.ForAddress(serverUrl, new GrpcChannelOptions
    {
        HttpHandler = new SocketsHttpHandler()
        {
            EnableMultipleHttp2Connections = true
        }
    });
    var stopWatch = Stopwatch.StartNew();
    var response = new ResponseModel();
    var concurrentJobs = new List<Task>();
    for (var i = 0; i < count; i++)
    {
        var client = new Monitor.MonitorClient(channel);
        concurrentJobs.Add(Task.Run(() =>
        {
            client.GetPerformance(new PerformanceStatusRequest
            {
                ClientName = $"client {i + 1}"
            });
        }));
    }
    Task.WaitAll(concurrentJobs.ToArray());
    response.RequestProcessingTime = stopWatch.ElapsedMilliseconds;
    return response;
})
.WithName("GetDataFromMultipleConnections")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}