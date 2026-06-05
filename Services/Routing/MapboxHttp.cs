namespace SafeCity.Services.Routing;

/// <summary>
/// Shared HTTP error mapping for the Mapbox services. Turns the two user-facing failure
/// modes (401 bad/missing token, 429 rate limit) into a friendly RoutingUnavailableException
/// so callers never crash on them.
/// </summary>
internal static class MapboxHttp
{
    public static void EnsureAvailable(HttpResponseMessage response)
    {
        var code = (int)response.StatusCode;
        if (code == 401)
            throw new RoutingUnavailableException(
                "Map service rejected the request — check the Mapbox token.");
        if (code == 429)
            throw new RoutingUnavailableException(
                "Map service is busy, try again shortly.");
    }
}
