using System.Globalization;
using System.Text.Json;
using SafeCity.Models;

namespace SafeCity.Services.Routing;

/// <summary>
/// PATTERN 6 (Adapter — continued alongside GeminiAssistantAdapter and MapboxMapAdapter).
/// Target:  app-domain types — GeoCoordinate, GeocodingResult
/// Adaptee: Mapbox JSON responses (Directions, Geocoding v6, Isochrone)
///
/// Mapbox uses [longitude, latitude] order everywhere — both in request paths and in
/// response geometry. The app domain always stores GeoCoordinate(Lat, Lon). The two
/// helpers below (FromLngLat for parsing, ToLngLat for request building) are the only
/// place in the codebase that performs this axis swap — so the inversion cannot be
/// mixed up in callers.
/// </summary>
public static class MapboxResponseAdapter
{
    // ── Canonical coordinate conversion ───────────────────────────────────────────

    /// <summary>Converts a Mapbox [longitude, latitude] pair to GeoCoordinate(Lat, Lon).</summary>
    public static GeoCoordinate FromLngLat(double lng, double lat) => new(lat, lng);

    /// <summary>Formats a GeoCoordinate as the "lng,lat" string Mapbox request paths expect.</summary>
    public static string ToLngLat(GeoCoordinate c) =>
        $"{c.Lon.ToString("G", CultureInfo.InvariantCulture)},{c.Lat.ToString("G", CultureInfo.InvariantCulture)}";

    // ── Directions: GET /directions/v5/mapbox/{profile}/... ───────────────────────

    /// <summary>
    /// Extracts the route polyline from a Directions response.
    /// routes[0].geometry is a GeoJSON LineString (geometries=geojson); each point is
    /// [lng, lat] mapped to GeoCoordinate via FromLngLat.
    /// </summary>
    public static IReadOnlyList<GeoCoordinate> ParseRouteCoordinates(JsonElement root)
    {
        if (!root.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
            return [];

        var first = routes[0];
        if (!first.TryGetProperty("geometry", out var geometry) ||
            !geometry.TryGetProperty("coordinates", out var coords))
            return [];

        return ParsePointArray(coords);
    }

    // ── Geocoding v6: GET /search/geocode/v6/{forward|reverse} ────────────────────

    /// <summary>
    /// Extracts labelled place results from a Geocoding v6 GeoJSON FeatureCollection.
    /// Each Point feature's [lng, lat] geometry plus properties.full_address (falling
    /// back to name) is extracted.
    /// </summary>
    public static IReadOnlyList<GeocodingResult> ParseGeocodingResults(JsonElement root)
    {
        var results = new List<GeocodingResult>();
        if (!root.TryGetProperty("features", out var features)) return results;

        foreach (var feature in features.EnumerateArray())
        {
            if (!TryGetPointCoord(feature, out var coord)) continue;
            if (!feature.TryGetProperty("properties", out var props)) continue;

            var label =
                GetString(props, "full_address") ??
                GetString(props, "place_formatted") ??
                GetString(props, "name") ??
                string.Empty;

            if (!string.IsNullOrEmpty(label))
                results.Add(new GeocodingResult(label, coord));
        }
        return results;
    }

    // ── Isochrone: GET /isochrone/v1/mapbox/{profile}/... ─────────────────────────

    /// <summary>
    /// Extracts the outer polygon ring from the first isochrone feature.
    /// geometry.coordinates = [outerRing, ...holes]; we only need index 0.
    /// </summary>
    public static IReadOnlyList<GeoCoordinate> ParseIsochronePolygon(JsonElement root)
    {
        if (!root.TryGetProperty("features", out var features) || features.GetArrayLength() == 0)
            return [];

        var first = features[0];
        if (!first.TryGetProperty("geometry", out var geometry) ||
            !geometry.TryGetProperty("coordinates", out var rings) ||
            rings.GetArrayLength() == 0)
            return [];

        return ParsePointArray(rings[0]);
    }

    // ── Private helpers ────────────────────────────────────────────────────────────

    private static IReadOnlyList<GeoCoordinate> ParsePointArray(JsonElement points)
    {
        var result = new List<GeoCoordinate>();
        foreach (var point in points.EnumerateArray())
        {
            var parts = point.EnumerateArray().ToArray();
            if (parts.Length >= 2)
                result.Add(FromLngLat(parts[0].GetDouble(), parts[1].GetDouble()));
        }
        return result;
    }

    private static bool TryGetPointCoord(JsonElement feature, out GeoCoordinate coord)
    {
        coord = default;
        if (!feature.TryGetProperty("geometry", out var geo)) return false;
        if (!geo.TryGetProperty("coordinates", out var coords)) return false;
        var parts = coords.EnumerateArray().ToArray();
        if (parts.Length < 2) return false;
        coord = FromLngLat(parts[0].GetDouble(), parts[1].GetDouble());
        return true;
    }

    private static string? GetString(JsonElement obj, string name) =>
        obj.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;
}
