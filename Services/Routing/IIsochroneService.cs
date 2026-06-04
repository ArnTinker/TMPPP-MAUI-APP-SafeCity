using SafeCity.Models;

namespace SafeCity.Services.Routing;

/// <summary>Calculates reachability areas (isochrones) around a geographic point.</summary>
public interface IIsochroneService
{
    /// <summary>
    /// Returns the polygon exterior ring of the area reachable within
    /// <paramref name="minutes"/> of travel from <paramref name="center"/>.
    /// </summary>
    Task<IReadOnlyList<GeoCoordinate>> GetReachableAreaAsync(
        GeoCoordinate center,
        int minutes,
        TransportProfile profile = TransportProfile.Walking,
        CancellationToken ct = default);
}
