using SafeCity.Models;

namespace SafeCity.Services.Routing;

/// <summary>Calculates a navigable route between two geographic coordinates.</summary>
public interface IRoutingService
{
    /// <summary>
    /// Returns the route polyline as an ordered list of coordinates from
    /// <paramref name="from"/> to <paramref name="to"/> using the given transport profile.
    /// </summary>
    Task<IReadOnlyList<GeoCoordinate>> GetRouteAsync(
        GeoCoordinate from,
        GeoCoordinate to,
        TransportProfile profile = TransportProfile.Walking,
        CancellationToken ct = default);
}
