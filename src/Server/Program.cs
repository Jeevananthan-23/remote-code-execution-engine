using MassTransit;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Shared;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var redis = ConnectionMultiplexer.Connect("redis-server:6379"); // Update with your Redis connection
var db = redis.GetDatabase();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:5173") 
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.AddMassTransit(busConfig =>
{
    busConfig.SetKebabCaseEndpointNameFormatter();

    busConfig.UsingRabbitMq((context, config) =>
    {
        config.Host(new Uri(builder.Configuration["MessageBroker:Host"]!), h =>
        {
            h.Username(builder.Configuration["MessageBroker:Username"]);
            h.Password(builder.Configuration["MessageBroker:Password"]);
        });

        config.ConfigureEndpoints(context);
    });
});


var app = builder.Build();

app.UseCors("AllowAnyOrigins");

var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.MapPost("/submit", async ([FromBody] SubmitRequest request, IPublishEndpoint publish) =>
{
    try
    {
        var folder = RandomHexString(10);

        await publish.Publish<ApiBody>(new
        {
            Src = request.Src,
            Input = request.Input,
            Lang = request.Lang,
            TimeOut = request.TimeOut,
            Folder = folder
        });

        var responseUrl = $"http://localhost:5000/results/{folder}";
        return Results.Accepted(responseUrl, folder);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
});

app.MapGet("/results/{id}", async (string id) =>
{
    try
    {
        var status = await db.StringGetAsync(id);

        if (status.IsNullOrEmpty)
        {
            return Results.Accepted(value: "Queued");
        }
        else if (status == "Processing")
        {
            return Results.Accepted(value: "Processing");
        }
        else
        {
            var result = JsonSerializer.Deserialize<object>(status);
            return Results.Ok(result);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
});



app.Run();

// Utility methods

string RandomHexString(int length)
{
    using var rng = RandomNumberGenerator.Create();
    var bytes = new byte[length];
    rng.GetBytes(bytes);
    return BitConverter.ToString(bytes).Replace("-", "").ToLower();
}

public class SubmitRequest
{
    public string Src { get; set; }
    public string Input { get; set; }
    public string Lang { get; set; }
    public int TimeOut { get; set; }
}

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(SubmitRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
