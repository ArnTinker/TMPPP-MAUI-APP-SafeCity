using System.Globalization;
using System.Text.Json;
using SafeCity.Models;

namespace SafeCity.Services.Routing;

/// <summary>
/// Mapbox Geocoding v6 implementation of IGeocodingService.
/// GET /search/geocode/v6/forward (q=) and /search/geocode/v6/reverse (longitude=,latitude=).
/// Token is passed as the access_token query parameter (no header → no CORS preflight).
/// </summary>
public sealed class MapboxGeocodingService : IGeocodingService
{
    private readonly HttpClient _http;

    public MapboxGeocodingService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("Mapbox");
    }

    public async Task<IReadOnlyList<GeocodingResult>> SearchAsync(
        string text,
        GeoCoordinate? focusPoint = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];

        var token = Uri.EscapeDataString(AppConfig.MapboxToken ?? string.Empty);
        var url   = $"/search/geocode/v6/forward?q={Uri.EscapeDataString(text)}" +
                    $"&country=md&limit=5&access_token={token}";

        if (focusPoint.HasValue)
            url += $"&proximity={Uri.EscapeDataString(MapboxResponseAdapter.ToLngLat(focusPoint.Value))}";

        var response = await _http.GetAsync(url, ct);
        MapboxHttp.EnsureAvailable(response);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return MapboxResponseAdapter.ParseGeocodingResults(doc.RootElement);
    }

    public async Task<GeocodingResult?> ReverseAsync(GeoCoordinate coord, CancellationToken ct = default)
    {
        var token = Uri.EscapeDataString(AppConfig.MapboxToken ?? string.Empty);
        var lon   = coord.Lon.ToString("G", CultureInfo.InvariantCulture);
        var lat   = coord.Lat.ToString("G", CultureInfo.InvariantCulture);
        var url   = $"/search/geocode/v6/reverse?longitude={lon}&latitude={lat}&access_token={token}";

        var response = await _http.GetAsync(url, ct);
        MapboxHttp.EnsureAvailable(response);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var results = MapboxResponseAdapter.ParseGeocodingResults(doc.RootElement);
        return results.Count > 0 ? results[0] : null;
    }
}
