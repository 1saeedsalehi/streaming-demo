namespace FlightStreamingDemo.Models;

public record FlightResult(
    string Provider,
    string From,
    string To,
    DateOnly Date,
    string FlightNumber,
    decimal PriceUsd
);
