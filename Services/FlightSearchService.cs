using FlightStreamingDemo.Models;
using FlightStreamingDemo.Providers;
using System.Runtime.CompilerServices;

namespace FlightStreamingDemo.Services;

public sealed class FlightSearchService(IEnumerable<IFlightProvider> providers)
{
    private readonly IReadOnlyList<IFlightProvider> _providers = providers.ToList();

    public async IAsyncEnumerable<FlightResult> SearchFlightsStream(
        string from,
        string to,
        DateOnly date,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var tasks = _providers
            .Select(p => p.SearchAsync(from, to, date, cancellationToken))
            .ToList();

        while (tasks.Count > 0)
        {
            var finished = await Task.WhenAny(tasks);
            tasks.Remove(finished);

            var providerResults = await finished;
            foreach (var result in providerResults)
            {
                yield return result;
            }
        }
    }
}
