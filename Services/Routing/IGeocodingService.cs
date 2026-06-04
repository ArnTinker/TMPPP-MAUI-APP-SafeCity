using SafeCity.Models;

namespace SafeCity.Services.Routing;

/// <summary>Address ↔ coordinate conversion (forward and reverse geocoding).</summary>
public interface IGeocodingService
{
    /// <summary>
    /// Returns places matching <paramref name="text"/>,
    /// optionally biased toward <paramref name="focusPoint"/>.
    /// </summary>
    Task<IReadOnlyList<GeocodingResult>> SearchAsync(
        string text,
        GeoCoordinate? focusPoint = null,
        CancellationToken ct = default);

    /// <summary>Returns the best street address for a coordinate, or null if none found.</summary>
    Task<GeocodingResult?> ReverseAsync(GeoCoordinate coord, CancellationToken ct = default);
}
