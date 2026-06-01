using SafeCity.Models;

namespace SafeCity.Patterns.Behavioral.Strategy;

/// <summary>
/// PATTERN: Strategy — Strategy interface.
/// Justification: The news feed supports four orderings (distance, recency, severity,
/// popularity). Each is a distinct algorithm; wrapping them in interchangeable strategy
/// objects lets the user switch at runtime without touching the ViewModel or feed logic.
/// </summary>
public interface IFeedSortStrategy
{
    string DisplayName { get; }
    IEnumerable<Incident> Sort(IEnumerable<Incident> incidents, double userLat = 0, double userLon = 0);
}
