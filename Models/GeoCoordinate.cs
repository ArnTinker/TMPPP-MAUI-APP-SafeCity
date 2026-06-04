namespace SafeCity.Models;

/// <summary>
/// App-domain geographic coordinate. Lat is always first (geographic convention).
/// ORS responses use [lon, lat] order — use OrsAdapter.FromLonLat() to convert.
/// </summary>
public readonly record struct GeoCoordinate(double Lat, double Lon);
