using System.Text.Json;
using FlightStreamingDemo.Services;
using FlightStreamingDemo.Providers;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
// Register providers and search service
builder.Services.AddSingleton<IFlightProvider, KLMFlightProvider>();
builder.Services.AddSingleton<IFlightProvider, EmiratesFlightProvider>();
builder.Services.AddSingleton<IFlightProvider, LufthansaFlightProvider>();
builder.Services.AddSingleton<FlightSearchService>();

var app = builder.Build();

// Serve the static frontend from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// NDJSON streaming endpoint: emits one JSON object per line as results arrive
app.MapGet("/api/flights/search", async (
    HttpContext http,
    [FromServices] FlightSearchService searchService,
    string from,
    string to,
    DateOnly date,
    CancellationToken ct) =>
{
    http.Response.Headers.CacheControl = "no-store";
    http.Response.ContentType = "application/x-ndjson"; // NDJSON

    // Use web defaults (camelCase) for property names to match frontend
    var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    await foreach (var result in searchService.SearchFlightsStream(from, to, date, ct))
    {
        var json = JsonSerializer.Serialize(result, jsonOptions);
        await http.Response.WriteAsync(json + "\n", ct);
        await http.Response.Body.FlushAsync(ct);
    }
});

app.Run();
