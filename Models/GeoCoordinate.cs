namespace SafeCity.Models;

/// <summary>
/// App-domain geographic coordinate. Lat is always first (geographic convention).
/// Mapbox uses [lng, lat] order — use MapboxResponseAdapter.FromLngLat()/ToLngLat() to convert.
/// </summary>
public readonly record struct GeoCoordinate(double Lat, double Lon);
