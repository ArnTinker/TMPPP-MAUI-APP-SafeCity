using System.Text.Json;
using SafeCity.Models;

namespace SafeCity.Services.Routing;

/// <summary>
/// PATTERN 6 (Adapter — continued alongside GeminiAssistantAdapter and MapMdAdapter).
/// Target:  app-domain types — GeoCoordinate, GeocodingResult
/// Adaptee: ORS GeoJSON FeatureCollection responses (all three endpoint families)
///
/// All ORS coordinates arrive as [longitude, latitude]. App domain always stores
/// GeoCoordinate(Lat, Lon). The single static method FromLonLat() is the only place
/// in the codebase that performs this axis swap — so the inversion cannot be mixed
/// up in callers.
/// </summary>
public static class OrsAdapter
{
    // ── Canonical coordinate conversion ───────────────────────────────────────────

    /// <summary>
    /// Converts an ORS [longitude, latitude] pair to the app's GeoCoordinate(Lat, Lon).
    /// All ORS response parsers in this class call this method — never swap axes elsewhere.
    /// </summary>
    public static GeoCoordinate FromLonLat(double lon, double lat) => new(lat, lon);

    // ── Directions ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the route polyline from a GeoJSON FeatureCollection returned by
    /// POST /v2/directions/{profile}/geojson.
    /// The first feature's LineString coordinates (which may include an elevation
    /// third element) are mapped to GeoCoordinate via FromLonLat.
    /// </summary>
    public static IReadOnlyList<GeoCoordinate> ParseRouteCoordinates(JsonElement root)
    {
        if (!TryGetFirstGeometry(root, out var geometry)) return [];

        var result = new List<GeoCoordinate>();
        foreach (var point in geometry.GetProperty("coordinates").EnumerateArray())
        {
            var parts = point.EnumerateArray().ToArray();
            if (parts.Length >= 2)
                result.Add(FromLonLat(parts[0].GetDouble(), parts[1].GetDouble()));
        }
        return result;
    }

    // ── Geocoding ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts labelled place results from a Pelias GeoJSON FeatureCollection
    /// returned by GET /geocode/search or GET /geocode/reverse.
    /// Each Point feature's [lon, lat] coordinates and properties.label are extracted.
    /// </summary>
    public static IReadOnlyList<GeocodingResult> ParseGeocodingResults(JsonElement root)
    {
        var results = new List<GeocodingResult>();
        if (!root.TryGetProperty("features", out var features)) return results;

        foreach (var feature in features.EnumerateArray())
        {
            if (!TryGetPointCoord(feature, out var coord)) continue;

            var label = string.Empty;
            if (feature.TryGetProperty("properties", out var props) &&
                props.TryGetProperty("label", out var lbl))
                label = lbl.GetString() ?? string.Empty;

            if (!string.IsNullOrEmpty(label))
                results.Add(new GeocodingResult(label, coord));
        }
        return results;
    }

    // ── Isochrones ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the outer polygon ring from the first isochrone feature returned by
    /// POST /v2/isochrones/{profile}.
    /// Polygon.coordinates = [outerRing, ...holes]; we only need index 0.
    /// </summary>
    public static IReadOnlyList<GeoCoordinate> ParseIsochronePolygon(JsonElement root)
    {
        if (!TryGetFirstGeometry(root, out var geometry)) return [];

        var coordArrays = geometry.GetProperty("coordinates");
        if (coordArrays.GetArrayLength() == 0) return [];

        var outerRing = coordArrays[0];
        var result = new List<GeoCoordinate>();
        foreach (var point in outerRing.EnumerateArray())
        {
            var parts = point.EnumerateArray().ToArray();
            if (parts.Length >= 2)
                result.Add(FromLonLat(parts[0].GetDouble(), parts[1].GetDouble()));
        }
        return result;
    }

    // ── Private helpers ────────────────────────────────────────────────────────────

    private static bool TryGetFirstGeometry(JsonElement root, out JsonElement geometry)
    {
        geometry = default;
        if (!root.TryGetProperty("features", out var features)) return false;
        var arr = features.EnumerateArray().ToArray();
        if (arr.Length == 0) return false;
        return arr[0].TryGetProperty("geometry", out geometry);
    }

    private static bool TryGetPointCoord(JsonElement feature, out GeoCoordinate coord)
    {
        coord = default;
        if (!feature.TryGetProperty("geometry", out var geo)) return false;
        if (!geo.TryGetProperty("coordinates", out var coords)) return false;
        var parts = coords.EnumerateArray().ToArray();
        if (parts.Length < 2) return false;
        coord = FromLonLat(parts[0].GetDouble(), parts[1].GetDouble());
        return true;
    }
}
