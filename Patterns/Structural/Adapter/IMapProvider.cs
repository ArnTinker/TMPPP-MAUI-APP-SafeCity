using SafeCity.Models;

namespace SafeCity.Patterns.Structural.Adapter;

/// <summary>
/// Map surface contract — abstracts Microsoft.Maui.Controls.Maps or any other provider
/// so the rest of the app never imports the maps namespace directly.
/// </summary>
public interface IMapProvider
{
    void CenterOn(double lat, double lon, double zoomKm = 5);
    void PlaceMarker(Incident incident);
    void ClearMarkers();
}
