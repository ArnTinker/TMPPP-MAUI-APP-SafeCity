using System.Globalization;
using System.Text.Json;
using SafeCity.Models;

namespace SafeCity.Services.Routing;

/// <summary>
/// Mapbox implementation of IRoutingService.
/// Calls GET /directions/v5/mapbox/{profile}/{lng,lat};{lng,lat} and delegates JSON
/// parsing to MapboxResponseAdapter. The token is passed as a query parameter, so there
/// is no Authorization header and therefore no CORS preflight.
/// </summary>
public sealed class MapboxRoutingService : IRoutingService
{
    private readonly HttpClient _http;

    public MapboxRoutingService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("Mapbox");
    }

    public async Task<IReadOnlyList<GeoCoordinate>> GetRouteAsync(
        GeoCoordinate from,
        GeoCoordinate to,
        TransportProfile profile = TransportProfile.Walking,
        CancellationToken ct = default)
    {
        var coords = $"{MapboxResponseAdapter.ToLngLat(from)};{MapboxResponseAdapter.ToLngLat(to)}";
        var token  = Uri.EscapeDataString(AppConfig.MapboxToken ?? string.Empty);
        var url    = $"/directions/v5/mapbox/{profile.ToMapboxProfile()}/{Uri.EscapeDataString(coords)}" +
                     $"?geometries=geojson&overview=full&access_token={token}";

        var response = await _http.GetAsync(url, ct);
        MapboxHttp.EnsureAvailable(response);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return MapboxResponseAdapter.ParseRouteCoordinates(doc.RootElement);
    }
}
