namespace SafeCity.Services.Routing;

/// <summary>Thrown when the ORS API responds with HTTP 429 (rate-limited).</summary>
public sealed class RoutingUnavailableException : Exception
{
    public RoutingUnavailableException(string message) : base(message) { }
}
