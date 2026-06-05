using SafeCity.Models;

namespace SafeCity.Patterns.Structural.Adapter;

/// <summary>
/// Map surface contract — abstracts Mapbox GL JS (or any other provider) so the rest
/// of the app never imports a maps-specific namespace directly.
/// </summary>
public interface IMapProvider
{
    /// <summary>Fly the camera to a coordinate at the given zoom level.</summary>
    void CenterOn(double lat, double lon, double zoomLevel = 12);

    /// <summary>Place or refresh the yellow user-location dot.</summary>
    void SetUserLocation(double lat, double lon);

    /// <summary>Add or update a marker for the given incident.</summary>
    void PlaceMarker(Incident incident);

    /// <summary>Remove all incident markers from the map.</summary>
    void ClearMarkers();

    /// <summary>Draw (or update) a route polyline from the given coordinate list.</summary>
    void DrawRoute(IReadOnlyList<GeoCoordinate> coordinates);

    /// <summary>Clear any active route polyline and isochrone overlay.</summary>
    void ClearRoute();

    /// <summary>Draw (or update) an isochrone polygon from the given exterior ring.</summary>
    void DrawIsochrone(IReadOnlyList<GeoCoordinate> polygon);

    /// <summary>True once the underlying map tile layer has fully initialised.</summary>
    bool IsReady { get; }

    /// <summary>Fires with the incident ID when a marker is tapped.</summary>
    event Action<int>? MarkerTapped;

    /// <summary>Fires with an error message if the map fails to load.</summary>
    event Action<string>? MapLoadError;
}
