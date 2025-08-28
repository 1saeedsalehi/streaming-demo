using FlightStreamingDemo.Models;
using System.Runtime.CompilerServices;

namespace FlightStreamingDemo.Providers;

public sealed class KLMFlightProvider : IFlightProvider
{
    public string Name => "KLM";

    public async IAsyncEnumerable<FlightResult> SearchAsync(string from, string to, DateOnly date, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var random = Random.Shared;
        await Task.Delay(random.Next(2000, 5000), cancellationToken);

        var count = random.Next(2, 4);
        var results = new List<FlightResult>(count);
        for (var i = 0; i < count; i++)
        {
            var price = Math.Round((decimal)(300 + random.NextDouble() * 700), 2);
            var flightNumber = "KL" + random.Next(100, 999);
            var result = new FlightResult(Name, from.ToUpperInvariant(), to.ToUpperInvariant(), date, flightNumber, price);
            yield return result;
        }
    }
}
