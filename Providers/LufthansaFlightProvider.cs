using FlightStreamingDemo.Models;

namespace FlightStreamingDemo.Providers;

public sealed class LufthansaFlightProvider : IFlightProvider
{
    public string Name => "Lufthansa";

    public async IAsyncEnumerable<FlightResult> SearchAsync(string from, string to, DateOnly date, CancellationToken cancellationToken)
    {
        var random = Random.Shared;
        await Task.Delay(random.Next(200, 3000), cancellationToken);

        var count = random.Next(2, 6);
        var results = new List<FlightResult>(count);
        for (var i = 0; i < count; i++)
        {
            var price = Math.Round((decimal)(300 + random.NextDouble() * 700), 2);
            var flightNumber = "LH" + random.Next(100, 999);
            var result = new FlightResult(Name, from.ToUpperInvariant(), to.ToUpperInvariant(), date, flightNumber, price);
            yield return result;
        }
    }
}
