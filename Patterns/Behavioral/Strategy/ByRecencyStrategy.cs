using SafeCity.Models;

namespace SafeCity.Patterns.Behavioral.Strategy;

/// <summary>PATTERN: Strategy — Sort by most recently created.</summary>
public class ByRecencyStrategy : IFeedSortStrategy
{
    public string DisplayName => "Recent";

    public IEnumerable<Incident> Sort(IEnumerable<Incident> incidents, double userLat = 0, double userLon = 0) =>
        incidents.OrderByDescending(i => i.CreatedAt);
}
