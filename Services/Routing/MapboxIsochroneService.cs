using System.Text.Json;
using SafeCity.Models;

namespace SafeCity.Services.Routing;

/// <summary>
/// Mapbox implementation of IIsochroneService.
/// Calls GET /isochrone/v1/mapbox/{profile}/{lng,lat} with contours_minutes and polygons=true, and
/// delegates JSON parsing to MapboxResponseAdapter. Token via access_token query parameter.
/// </summary>
public sealed class MapboxIsochroneService : IIsochroneService
{
    private readonly HttpClient _http;

    public MapboxIsochroneService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("Mapbox");
    }

    public async Task<IReadOnlyList<GeoCoordinate>> GetReachableAreaAsync(
        GeoCoordinate center,
        int minutes,
        TransportProfile profile = TransportProfile.Walking,
        CancellationToken ct = default)
    {
        var coord = Uri.EscapeDataString(MapboxResponseAdapter.ToLngLat(center));
        var token = Uri.EscapeDataString(AppConfig.MapboxToken ?? string.Empty);
        var url   = $"/isochrone/v1/mapbox/{profile.ToMapboxProfile()}/{coord}" +
                    $"?contours_minutes={Math.Clamp(minutes, 1, 60)}&polygons=true&access_token={token}";

        var response = await _http.GetAsync(url, ct);
        MapboxHttp.EnsureAvailable(response);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return MapboxResponseAdapter.ParseIsochronePolygon(doc.RootElement);
    }
}
