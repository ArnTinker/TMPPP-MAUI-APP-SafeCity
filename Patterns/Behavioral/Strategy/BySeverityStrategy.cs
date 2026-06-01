using SafeCity.Models;

namespace SafeCity.Patterns.Behavioral.Strategy;

/// <summary>PATTERN: Strategy — Sort by descending severity (Critical first).</summary>
public class BySeverityStrategy : IFeedSortStrategy
{
    public string DisplayName => "Severity";

    public IEnumerable<Incident> Sort(IEnumerable<Incident> incidents, double userLat = 0, double userLon = 0) =>
        incidents.OrderByDescending(i => (int)i.Severity);
}
