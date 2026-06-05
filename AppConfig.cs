using SafeCity.Helpers;

namespace SafeCity;

/// <summary>Central configuration constants and feature flags.</summary>
public static class AppConfig
{
    public const string BackendBaseUrl = "https://safecity-api.onrender.com";

    // Feature flags
    public const bool   EnableGeminiAssistant = true;
    public const bool   EnableBackendSync     = false;
    public const double NearbyRadiusKm        = 10.0;
    public const int    MaxMediaAttachments    = 5;
    public const int    FeedPageSize           = 20;

    // Map defaults — Chișinău city centre
    public const double DefaultLat = 47.0105;
    public const double DefaultLon = 28.8638;

    // Mapbox public token — read from .env at runtime, never hardcoded.
    // Powers the GL JS map, Directions, Geocoding and Isochrone APIs.
    public static string? MapboxToken => EnvHelper.Get("MAPBOX_TOKEN");

    // Optional custom Mapbox style URL (e.g. mapbox://styles/you/abc123).
    // Falls back to mapbox://styles/mapbox/dark-v11 inside mapbox.html when empty.
    public static string? MapboxStyleUrl => EnvHelper.Get("MAPBOX_STYLE_URL");
}
