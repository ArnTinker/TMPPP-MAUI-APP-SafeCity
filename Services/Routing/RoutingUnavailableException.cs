namespace SafeCity.Services.Routing;

/// <summary>Thrown when the Mapbox API responds with HTTP 401 (bad token) or 429 (rate-limited).</summary>
public sealed class RoutingUnavailableException : Exception
{
    public RoutingUnavailableException(string message) : base(message) { }
}
