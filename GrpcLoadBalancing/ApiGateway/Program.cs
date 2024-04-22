using ApiGateway;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var addresses = builder.Configuration.GetSection("ServerAddresses").Get<List<string>>();
builder.Services.AddSingleton<IGrpcClientWrapper>(new GrpcClientWrapper(addresses));

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

app.MapPost("standard-client/{count}", async (int count, IGrpcClientWrapper clientWrapper) =>
{
    var stopWatch = Stopwatch.StartNew();
    var processedCount = await clientWrapper.SendDataViaStandardClient(count);
    return new ApiResponse
    {
        DataItemsProcessed = processedCount,
        RequestProcessingTime = stopWatch.ElapsedMilliseconds
    };
})
.WithName("PostDataViaStandardClient")
.WithOpenApi();

app.MapPost("load-balancer/{count}", async (int count, IGrpcClientWrapper clientWrapper) =>
{
    var stopWatch = Stopwatch.StartNew();
    var processedCount = await clientWrapper.SendDataViaLoadBalancer(count);
    return new ApiResponse
    {
        DataItemsProcessed = processedCount,
        RequestProcessingTime = stopWatch.ElapsedMilliseconds
    };
})
.WithName("PostDataViaLoadBalancer")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}