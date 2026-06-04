namespace SafeCity.Models;

/// <summary>A single result from a geocoding search or reverse-geocode lookup.</summary>
public sealed record GeocodingResult(string Label, GeoCoordinate Coordinate);
