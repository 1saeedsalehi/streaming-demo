using FlightStreamingDemo.Models;
using FlightStreamingDemo.Providers;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);
// Register providers and search service
builder.Services.AddSingleton<IFlightProvider, KLMFlightProvider>();
builder.Services.AddSingleton<IFlightProvider, EmiratesFlightProvider>();
builder.Services.AddSingleton<IFlightProvider, LufthansaFlightProvider>();

var app = builder.Build();

// Serve the static frontend from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// 
app.MapGet("/api/flights/search", async (
    HttpContext http,
    string from,
    string to,
    DateOnly date,
    CancellationToken ct) =>
{
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };


    http.Response.ContentType = "application/x-ndjson";
    var providers = http.RequestServices.GetServices<IFlightProvider>();
    var channel = Channel.CreateUnbounded<FlightResult>();

    // Producer tasks
    var tasks = providers.Select(async provider =>
    {
        await foreach (var result in provider.SearchAsync(from, to, date, ct).WithCancellation(ct))
        {
            await channel.Writer.WriteAsync(result, ct);
        }
    });

    // Consumer task
    var consumer = Task.Run(async () =>
    {
        await foreach (var result in channel.Reader.ReadAllAsync(ct))
        {
            await JsonSerializer.SerializeAsync(http.Response.Body, result, options, ct);
            await http.Response.Body.WriteAsync("\n"u8.ToArray(), ct);
            await http.Response.Body.FlushAsync(ct);
        }
    }, ct);

    await Task.WhenAll(tasks);
    channel.Writer.Complete();
    await consumer;
});

app.Run();
