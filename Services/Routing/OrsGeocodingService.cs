using System.Globalization;
using System.Text.Json;
using SafeCity.Models;

namespace SafeCity.Services.Routing;

/// <summary>
/// ORS (Pelias) implementation of IGeocodingService.
/// GET /geocode/search and GET /geocode/reverse use api_key= query param (not header).
/// </summary>
public sealed class OrsGeocodingService : IGeocodingService
{
    private readonly HttpClient _http;

    public OrsGeocodingService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("ORS");
    }

    public async Task<IReadOnlyList<GeocodingResult>> SearchAsync(
        string text,
        GeoCoordinate? focusPoint = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];

        var key = Uri.EscapeDataString(AppConfig.OrsApiKey ?? string.Empty);
        var url  = $"/geocode/search?api_key={key}&text={Uri.EscapeDataString(text)}&boundary.country=MD";

        if (focusPoint.HasValue)
        {
            var lon = focusPoint.Value.Lon.ToString("G", CultureInfo.InvariantCulture);
            var lat = focusPoint.Value.Lat.ToString("G", CultureInfo.InvariantCulture);
            url += $"&focus.point.lon={lon}&focus.point.lat={lat}";
        }

        var response = await _http.GetAsync(url, ct);
        EnsureNotRateLimited(response);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return OrsAdapter.ParseGeocodingResults(doc.RootElement);
    }

    public async Task<GeocodingResult?> ReverseAsync(GeoCoordinate coord, CancellationToken ct = default)
    {
        var key = Uri.EscapeDataString(AppConfig.OrsApiKey ?? string.Empty);
        var lon = coord.Lon.ToString("G", CultureInfo.InvariantCulture);
        var lat = coord.Lat.ToString("G", CultureInfo.InvariantCulture);
        var url  = $"/geocode/reverse?api_key={key}&point.lon={lon}&point.lat={lat}";

        var response = await _http.GetAsync(url, ct);
        EnsureNotRateLimited(response);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var results = OrsAdapter.ParseGeocodingResults(doc.RootElement);
        return results.Count > 0 ? results[0] : null;
    }

    private static void EnsureNotRateLimited(HttpResponseMessage response)
    {
        if ((int)response.StatusCode == 429)
            throw new RoutingUnavailableException(
                "Routing temporarily unavailable, try again shortly.");
    }
}
