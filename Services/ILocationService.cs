namespace SafeCity.Services;

public enum LocationStatus { Success, PermissionDenied, GpsOff, Unavailable }

/// <summary>
/// Carries the result of a location request, distinguishing success from the three
/// distinct failure modes so callers can show actionable messages instead of silently failing.
/// </summary>
public sealed record LocationResult(
    LocationStatus Status,
    double Lat = 0,
    double Lon = 0,
    string? ErrorMessage = null)
{
    public bool IsSuccess    => Status == LocationStatus.Success;
    public bool HasCoords    => Lat != 0 || Lon != 0;
}

public interface ILocationService
{
    /// <summary>
    /// Returns a LocationResult. On failure the result carries an ErrorMessage the UI
    /// can display directly. Never throws — all exceptions are mapped to LocationStatus values.
    /// </summary>
    Task<LocationResult> GetCurrentLocationAsync();

    Task<string> ReverseGeocodeAsync(double lat, double lon);
}
