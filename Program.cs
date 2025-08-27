using FlightStreamingDemo.Providers;

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
    //TODO: implement the flight search logic here
    //Tip: use NDJSON for outpyt emits one JSON object per line as results arrive
    http.Response.ContentType = "application/x-ndjson";
    var providers = http.RequestServices.GetServices<IFlightProvider>();
    var tasks = providers.Select(async provider =>
    {
        await foreach (var result in provider.SearchAsync(from, to, date, ct))
        {
            await http.Response.WriteAsJsonAsync(result, ct);

            // Write a newline character to separate this JSON object from the next one.
            await http.Response.Body.WriteAsync("\n"u8.ToArray(), ct);
        }
    });

    await Task.WhenAll(tasks);
});

app.Run();
