using FlightStreamingDemo.Models;

namespace FlightStreamingDemo.Providers;

public interface IFlightProvider
{
    string Name { get; }
    IAsyncEnumerable<FlightResult> SearchAsync(string from, string to, DateOnly date, CancellationToken cancellationToken);
}
