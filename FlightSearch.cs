using System.Runtime.CompilerServices;

namespace FlightStreamingDemo;

public record FlightResult(
    string Provider,
    string From,
    string To,
    DateOnly Date,
    string FlightNumber,
    decimal PriceUsd
);

public static class FlightSearch
{
    public static async IAsyncEnumerable<FlightResult> SearchFlightsStream(
        string from,
        string to,
        DateOnly date,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var providers = new[] { "KLM", "Emirates", "Lufthansa" };

        var tasks = providers
            .Select(p => SimulateProviderSearchAsync(p, from, to, date, cancellationToken))
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

    private static async Task<List<FlightResult>> SimulateProviderSearchAsync(
        string provider,
        string from,
        string to,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var random = Random.Shared;
        var delayMs = random.Next(200, 3000);
        await Task.Delay(delayMs, cancellationToken);

        // Simulate a variable number of flights per provider
        var count = random.Next(2, 6); // 2 to 5 flights
        var results = new List<FlightResult>(capacity: count);

        for (var i = 0; i < count; i++)
        {
            var price = Math.Round((decimal)(300 + random.NextDouble() * 700), 2);
            var flightNumber = provider[..Math.Min(provider.Length, 2)].ToUpperInvariant() + random.Next(100, 999);

            results.Add(new FlightResult(
                Provider: provider,
                From: from.ToUpperInvariant(),
                To: to.ToUpperInvariant(),
                Date: date,
                FlightNumber: flightNumber,
                PriceUsd: price
            ));
        }

        return results;
    }
}
