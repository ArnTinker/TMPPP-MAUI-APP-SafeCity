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

    // map.md token — read from .env at runtime, never hardcoded
    public static string? MapMdToken => EnvHelper.Get("MAPMD_TOKEN");

    // OpenRouteService API key — read from .env at runtime, never hardcoded
    public static string? OrsApiKey => EnvHelper.Get("ORS_API_KEY");
}
