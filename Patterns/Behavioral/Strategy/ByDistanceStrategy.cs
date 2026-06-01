using SafeCity.Models;

namespace SafeCity.Patterns.Behavioral.Strategy;

/// <summary>PATTERN: Strategy — Sort by distance from the user's current position.</summary>
public class ByDistanceStrategy : IFeedSortStrategy
{
    public string DisplayName => "Nearest";

    public IEnumerable<Incident> Sort(IEnumerable<Incident> incidents, double userLat = 0, double userLon = 0) =>
        incidents.OrderBy(i => Haversine(userLat, userLon, i.Latitude, i.Longitude));

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
