using System.Globalization;
using System.Text.Json;
using SafeCity.Enums;
using SafeCity.Models;
using SafeCity.Patterns.Structural.Adapter;

namespace SafeCity.Services.Maps;

/// <summary>
/// PATTERN 6 (Adapter — Object Adapter, continued from GeminiAssistantAdapter).
/// Target:   IMapProvider   (what the rest of the app calls)
/// Adaptee:  MapboxProvider (raw Mapbox GL JS bridge)
/// This class translates domain-level IMapProvider calls into the Mapbox GL JS
/// string API expected by mapbox.html. Swapping the map provider only requires a
/// new adapter — no ViewModel or Repository changes.
/// </summary>
public sealed class MapboxMapAdapter : IMapProvider
{
    private readonly MapboxProvider _provider;

    public bool IsReady => _provider.IsReady;

    public event Action<int>? MarkerTapped
    {
        add    => _provider.MarkerTapped += value;
        remove => _provider.MarkerTapped -= value;
    }

    public event Action<string>? MapLoadError
    {
        add    => _provider.MapLoadError += value;
        remove => _provider.MapLoadError -= value;
    }

    public MapboxMapAdapter(MapboxProvider provider) => _provider = provider;

    public void CenterOn(double lat, double lon, double zoomLevel = 12) =>
        _ = _provider.ExecuteAsync(
            $"window.setCenter({F(lat)},{F(lon)},{F(zoomLevel)});");

    public void SetUserLocation(double lat, double lon) =>
        _ = _provider.ExecuteAsync(
            $"window.setUserLocation({F(lat)},{F(lon)});");

    public void PlaceMarker(Incident incident)
    {
        var severity = incident.Severity.ToString();
        var resolved = incident.State == IncidentStateEnum.Resolved ? "true" : "false";
        _ = _provider.ExecuteAsync(
            $"window.addMarker({incident.Id},{F(incident.Latitude)},{F(incident.Longitude)},'{severity}',{resolved});");
    }

    public void ClearMarkers() =>
        _ = _provider.ExecuteAsync("window.clearMarkers();");

    public void DrawRoute(IReadOnlyList<GeoCoordinate> coordinates)
    {
        if (coordinates.Count == 0) { ClearRoute(); return; }
        var json = BuildLineStringGeojson(coordinates);
        _ = _provider.ExecuteAsync($"window.drawRoute({json});");
    }

    public void ClearRoute() =>
        _ = _provider.ExecuteAsync("window.clearRoute();");

    public void DrawIsochrone(IReadOnlyList<GeoCoordinate> polygon)
    {
        if (polygon.Count == 0) return;
        var json = BuildPolygonGeojson(polygon);
        _ = _provider.ExecuteAsync($"window.drawIsochrone({json});");
    }

    // ── GeoJSON builders ──────────────────────────────────────────────────────────

    // Builds a GeoJSON FeatureCollection with a single LineString feature.
    // Coordinates must be [lon, lat] for GeoJSON — note the reversal from GeoCoordinate.
    private static string BuildLineStringGeojson(IReadOnlyList<GeoCoordinate> coords)
    {
        var lonLat = coords.Select(c => new[] { c.Lon, c.Lat }).ToArray();
        return JsonSerializer.Serialize(new
        {
            type = "FeatureCollection",
            features = new[] {
                new {
                    type     = "Feature",
                    geometry = new { type = "LineString", coordinates = lonLat },
                    properties = new { }
                }
            }
        });
    }

    // Builds a GeoJSON FeatureCollection with a single Polygon feature (outer ring only).
    private static string BuildPolygonGeojson(IReadOnlyList<GeoCoordinate> ring)
    {
        var lonLat = ring.Select(c => new[] { c.Lon, c.Lat }).ToArray();
        return JsonSerializer.Serialize(new
        {
            type = "FeatureCollection",
            features = new[] {
                new {
                    type     = "Feature",
                    geometry = new { type = "Polygon", coordinates = new[] { lonLat } },
                    properties = new { }
                }
            }
        });
    }

    // Invariant-culture double → JS number literal (no comma decimal separator)
    private static string F(double v) =>
        v.ToString("G", CultureInfo.InvariantCulture);
}
