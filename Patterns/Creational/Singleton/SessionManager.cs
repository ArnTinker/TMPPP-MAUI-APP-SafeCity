using SafeCity.Models;

namespace SafeCity.Patterns.Creational.Singleton;

/// <summary>
/// PATTERN: Singleton
/// Justification: The app has exactly one active session at a time. Registering this
/// as a DI singleton guarantees every service reads the same user, token, and location
/// without passing them through call chains or using static fields.
/// </summary>
public class SessionManager
{
    public User? CurrentUser { get; set; }
    public string? AuthToken { get; set; }
    public double? LastKnownLat { get; set; }
    public double? LastKnownLon { get; set; }
    public bool LocationGranted { get; set; }

    public bool IsLoggedIn => CurrentUser is not null && AuthToken is not null;

    public void UpdateLocation(double lat, double lon)
    {
        LastKnownLat = lat;
        LastKnownLon = lon;
    }

    public void Clear()
    {
        CurrentUser = null;
        AuthToken = null;
        LastKnownLat = null;
        LastKnownLon = null;
        LocationGranted = false;
    }
}
