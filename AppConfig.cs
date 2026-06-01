namespace SafeCity;

/// <summary>Central configuration constants and feature flags.</summary>
public static class AppConfig
{
    // Replace with your Render.com URL once deployed
    public const string BackendBaseUrl = "https://safecity-api.onrender.com";

    // Feature flags
    public const bool EnableGeminiAssistant = true;
    public const bool EnableBackendSync     = false; // flip to true when backend is live
    public const double NearbyRadiusKm      = 10.0;
    public const int    MaxMediaAttachments  = 5;
    public const int    FeedPageSize         = 20;

    // Map default (center of a generic city — override once location is granted)
    public const double DefaultLat = 48.8566;
    public const double DefaultLon = 2.3522;
}
